using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using SocialManager.Attributes;

namespace SocialManager.Models
{
    public class FacebookPhotoViewModel
    {
        [Required]
        [FacebookMapping("id")]
        public string Id { get; set; }

        [Display(Name = "Name")]
        [FacebookMapping("name")]
        public string name { get; set; }

        [FacebookMapping("picture")]
        public string SmallPicture { get; set; }

        [FacebookMapping("source", parent = "images")]
        public string LargePicture { get; set; }

    }
}