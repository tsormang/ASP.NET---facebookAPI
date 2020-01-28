using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialManager.Models
{
    public class PermissionRequestViewModel
    {
        public PermissionRequestViewModel()
        {
            MissingPermissions = new List<FacebookPermissionRequest>();
        }

        public List<FacebookPermissionRequest> MissingPermissions { get; set; }

        public string redirectURI { get; set; }
    }

    public class FacebookPermissionRequest
    {
        public bool requested { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string permision_scope_value { get; set; }

        public bool granted { get; set; }
    }
}
