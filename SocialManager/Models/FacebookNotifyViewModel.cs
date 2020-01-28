using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using SocialManager.Extensions;

namespace SocialManager.Models
{
    public class FacebookNotifyViewModel
    {
        private Guid _instanceId = Guid.NewGuid();
        public string instanceId
        {
            get
            {
                return _instanceId.ToString();
            }
        }
        [DataMember(Name = "object")]
        public string Object { get; set; }
        [DataMember]
        public List<FacebookNotify_Entry> entry { get; set; }
    }

    public class FacebookNotify_Entry
    {
        [DataMember]
        public string uid { get; set; }
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public List<string> changed_fields { get; set; }
        [DataMember]
        public List<FacebookNotify_Change> changes { get; set; }
        [DataMember]
        public double time { get; set; }
        public DateTime LocalTime {
            get
            {
               return this.time.ConvertToLocalTime();
            }
        }
    }

    public class FacebookNotify_Change
    {
        [DataMember]
        public string field { get; set; }
        [DataMember]
        public FacebookNotify_Value value { get; set; }


    }

    public class FacebookNotify_Value
    {
        [DataMember]
        public string item { get; set; }
        [DataMember]
        public string verb { get; set; }
        [DataMember]
        public string parent_id { get; set; }
        [DataMember]
        public string post_id { get; set; }
        [DataMember]
        public string sender_id { get; set; }
        [DataMember]
        public double created_time { get; set; }
        public DateTime LocalCreatedTime
        {
            get
            {
                return this.created_time.ConvertToLocalTime();
            }
        }
    }
}