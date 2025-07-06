using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
