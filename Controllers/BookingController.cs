﻿using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Common.Utility;
using HotelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Stripe.Checkout;
using System.Buffers;
using System.Security.Claims;

namespace HotelBooking.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u=>u.Id==villaId,includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}"
            };
            options.LineItems.Add(new SessionLineItemOptions
            {
               PriceData=new SessionLineItemPriceDataOptions
               {
                   UnitAmount = (long)(booking.TotalCost * 100),
                   Currency = "usd",
                   ProductData = new SessionLineItemPriceDataProductDataOptions
                   {
                       Name = villa.Name
                   },
               },
               Quantity = 1

            });
            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Booking.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            //var domain = $"{Request.Scheme}://{Request.Host.Value}"; // No trailing "/"

            //var successUrl = $"{domain}/booking/BookingConfirmation?bookingId={booking.Id}";
            //var cancelUrl = $"{domain}/booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate:yyyy-MM-dd}&nights={booking.Nights}";

            // Optional: log them or use debugger to inspect
            //Console.WriteLine("Success URL: " + successUrl);
            //Console.WriteLine("Cancel URL: " + cancelUrl);

            //var options = new SessionCreateOptions
            //{
            //    LineItems = new List<SessionLineItemOptions>(),
            //    Mode = "payment",
            //    SuccessUrl = successUrl,
            //    CancelUrl = cancelUrl,
            //};
            //options.LineItems.Add(new SessionLineItemOptions
            //{
            //    PriceData = new SessionLineItemPriceDataOptions
            //    {
            //        UnitAmount = (long)(booking.TotalCost *100),
            //        Currency = "usd",
            //        ProductData=new SessionLineItemPriceDataProductDataOptions
            //        {
            //            Name = villa.Name
            //            //Image = new List<string> {domain + villa.ImageUrl},
            //        }
            //    },
            //    Quantity = 1,
            //});
            //var service = new SessionService();
            //Session session = service.Create(options);

            //_unitOfWork.Booking.UpdateStripePaymentId(booking.Id, session.Id,session.PaymentIntentId);
            //_unitOfWork.Save();


            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId,
                includeProperties: "User,Villa");
            if(bookingFromDb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if(session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved);
                    _unitOfWork.Booking.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            //Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId,
            //    includeProperties:"User,Villa");
            //if(bookingFromDb.Status == SD.StatusPending)
            //{
            //    var service = new SessionService();
            //    Session session = service.Get(bookingFromDb.StripePaymentIntentId);

            //    if(session.PaymentStatus == "paid")
            //    {
            //        _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved);
            //        _unitOfWork.Booking.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
            //        _unitOfWork.Save();
            //    }

            //}
            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId,
               includeProperties: "User,Villa");
            return View(bookingFromDb);
        }


        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;
            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objBookings = _unitOfWork.Booking.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }
            
                objBookings = objBookings.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            
            return Json(new {data=objBookings});
        }
        #endregion
    }
}
