using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using SocialManager.Attributes;

namespace SocialManager.Models
{
    public class GeoCodeLocationViewModel
    {
        [Display(Name = "Center Search on Location")]
        public string Name { get; set; }

        [Display(Name = "Coordinates")]
        public string Coordinates { get; set; }

    }
}