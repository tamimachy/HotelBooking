using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Common.Utility;
using HotelBooking.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;

namespace HotelBooking.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        public readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month-1,1);
        public readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month,1);
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending ||
            u.Status == SD.StatusCancelled);
            var countByCurrentMonth = totalBookings.Count(u => u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(u => u.BookingDate >= previousMonthStartDate && u.BookingDate <= currentMonthStartDate);
            RadialBarChartVM radialBarChartVM = new();
            int incresedDecreasedRatio = 100;
            if (countByPreviousMonth != 0)
            {
                incresedDecreasedRatio = Convert.ToInt32((countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100);
            }
            radialBarChartVM.TotalCount = totalBookings.Count();
            radialBarChartVM.CountInCurrentMonth = countByCurrentMonth;
            radialBarChartVM.HasRatioIncreased = currentMonthStartDate > previousMonthStartDate;
            radialBarChartVM.Series = new int[] { incresedDecreasedRatio };

            return Json(radialBarChartVM);
        }
    }
}
