using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using SocialManager.Attributes;

namespace SocialManager.Models
{
    public class FacebookPostViewModel
    {
        [Required]
        [FacebookMapping("id")]
        public string Id { get; set; }

        [FacebookMapping("created_time")]
        public DateTime Created_Time { get; set; }

        [FacebookMapping("id", parent = "from")]
        public string From_Id { get; set; }

        [FacebookMapping("name", parent = "from")]
        public string From_Name { get; set; }

        [FacebookMapping("url", parent = "from")]
        public string From_Picture_Url { get; set; }

        [FacebookMapping("story")]
        public string Story { get; set; }

        [FacebookMapping("message")]
        public string Message { get; set; }

        [FacebookMapping("picture")]
        public string Picture_Url { get; set; }

        [FacebookMapping("link")]
        public string Link { get; set; }

        [FacebookMapping("description")]
        public string Description { get; set; }

        [FacebookMapping("caption")]
        public string Caption { get; set; }

        [FacebookMapping("type")]
        public string Type { get; set; }

        [FacebookMapping("likes")]
        public dynamic Likes { get; set; }

        [FacebookMapping("comments")]
        public dynamic Comments { get; set; }

    }
}
