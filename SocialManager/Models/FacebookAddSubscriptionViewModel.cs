using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialManager.Models
{
    public class FacebookAddSubscriptionViewModel
    {

        public FacebookAddSubscriptionViewModel(string[] user_fields, 
                                                string[] page_fields, 
                                                string[] permission_fields)
        {
            this.UserSubscriptionFields = new List<string>(user_fields);
            this.PageSubscriptionFields = new List<string>(page_fields);
            this.PermissionSubscriptionFields = new List<string>(permission_fields);
        }

        
        public List<string> UserSubscriptionFields { get; set; }

        public List<string> PageSubscriptionFields { get; set; }

        public List<string> PermissionSubscriptionFields { get; set; }
    }
}