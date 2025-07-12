using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Services.Interface;
using HotelBooking.Domain.Entities;
using HotelBooking.Infrastructure.Data;
using HotelBooking.Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBooking.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villas = _villaService.GetAllVillas();
            return View(villas);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description)
            {
                ModelState.AddModelError("name", "The Description can not exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                _villaService.CreateVilla(obj);
                TempData["success"] = "The Villa has been Created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "The Villa could not be created!";
                return View();
            }
        }
        public IActionResult Update(int villaId)
        {
            Villa? obj =_villaService.GetVillaById(villaId);
            if (obj == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }
        [HttpPost]
        public IActionResult Update(Villa obj)
        {
           if (ModelState.IsValid && obj.Id>0)
            {
                _villaService.UpdateVilla(obj);
                TempData["success"] = "The Villa has been Updated Successfully!";
                return RedirectToAction(nameof(Index));
           }
            TempData["error"] = "The Villa Could not updated!";
            return View();
        }
        public IActionResult Delete(int villaId)
        {
            Villa? obj = _villaService.GetVillaById(villaId);
            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }
        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            bool deleted = _villaService.DeleteVilla(obj.Id);
            if (deleted)
            {
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Failed to delete the villa.";
            }
            return View();
        }
    }
}