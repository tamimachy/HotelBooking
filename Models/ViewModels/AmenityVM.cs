﻿using HotelBooking.Domain.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBooking.Web.Models.ViewModels
{
    public class AmenityVM
    {
        public Amenity? Amenity{ get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? VillaList { get; set; }
    }
}
