using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Common.Utility;
using HotelBooking.Application.Services.Implementation;
using HotelBooking.Application.Services.Interface;
using HotelBooking.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;
using System.Linq;

namespace HotelBooking.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {

            return Json(await _dashboardService.GetTotalBookingRadialChartData());
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            return Json(await _dashboardService.GetRegisteredUserChartData());
        }
        public async Task<IActionResult> GetRevenueChartData()
        {
            return Json(await _dashboardService.GetRevenueChartData());
        }
        public async Task<IActionResult> GetBookingsPieChartData()
        {
            return Json(await _dashboardService.GetBookingsPieChartData());
        }
        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await _dashboardService.GetTotalBookingRadialChartData());
        }
    }
}
