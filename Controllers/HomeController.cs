using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.Web.Models;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Web.Models.ViewModels;

namespace HotelBooking.Web.Controllers;

public class HomeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public HomeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        HomeVM homeVM = new()
        {
            VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
            Nights = 1,
            CheckInDate = DateOnly.FromDateTime(DateTime.Now),
        };
        return View(homeVM);
    }
    [HttpPost]
    public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
    {
        var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
        foreach (var villa in villaList)
        {
            if (villa.Id % 2 == 0)
            {
                villa.IsAvailable = false;
            }
        }
        HomeVM homeVM = new()
        {
            CheckInDate = checkInDate,
            Nights = nights,
            VillaList = villaList
        };
        return PartialView("_VillaList",homeVM);
    }
    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Error()
    {
        return View();
    }
}
