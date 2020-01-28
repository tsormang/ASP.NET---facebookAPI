using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SocialManager.Attributes;
namespace SocialManager.Models
{public class FacebookAlbumViewModel
    {
        [Required]
        [FacebookMapping("id")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Album Name")]
        [FacebookMapping("name")]
        public string Name { get; set; }

        [Display(Name = "Photo Count")]
        [FacebookMapping("count")]
        public long count { get; set; }

        [Display(Name = "Link")]
        [FacebookMapping("link")]
        public string link { get; set; }

        [FacebookMapping("url", parent = "picture")]
        public string ImageURL { get; set; }

        [FacebookMapping("privacy")]
        public string Privacy { get; set; }
    }
}