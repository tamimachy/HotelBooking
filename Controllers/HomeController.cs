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

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Error()
    {
        return View();
    }
}
