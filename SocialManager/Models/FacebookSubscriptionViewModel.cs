using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SocialManager.Attributes;

namespace SocialManager.Models
{
    public class FacebookSubscriptionViewModel
    {

        [FacebookMapping("object")]
        public string Type { get; set; }

        [FacebookMapping("callback_url")]
        public string callback_url { get; set; }

        [FacebookMapping("active")]
        public bool active { get; set; }

        [FacebookMapping("fields")]
        public dynamic fields { get; set; }
    }
}