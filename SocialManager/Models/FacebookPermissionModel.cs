using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialManager.Models
{
    [DataContract]
    public class PermissionResults
    {
        [DataMember(Name = "data")]
        public List<FacebookPermissionModel> data { get; set; }
    }

    [DataContract]
    public class FacebookPermissionModel
    {
        [DataMember(Name = "permission")]
        public string permission { get; set; }

        [DataMember(Name = "status")]
        public string status { get; set; }
    }
}