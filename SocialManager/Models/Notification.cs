using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SocialManager.Models
{
    public class NotificationItem
    {
        public NotificationItem(string json)
        {
            var item = JObject.Parse(json);
            type = (string)item["object"];

            entries = new List<NotificationEntry>();
            foreach(var entryJson in item["entry"])
            {
                var entry = new NotificationEntry
                    {
                        id = (string)entryJson["id"],
                        time = Facebook.DateTimeConvertor.
                        FromUnixTime((string)entryJson["time"]) 
                    };
                foreach (var changeJson in entryJson["changes"])
                {

                  var valueJson = changeJson["value"];
                  var valueItem = new NotificationValue()
                            {
                                item = (string)valueJson["item"],
                                verb = (string)valueJson["verb"],
                                parent_id = (string)valueJson["parent_id"],
                                sender_id = (string)valueJson["sender_id"],
                                created_time =  Facebook.DateTimeConvertor.
                                FromUnixTime((string)valueJson["created_time"]) 
                            };
                   var change =  new NotificationChange
                        {
                            field = (string)changeJson["field"],
                            value = valueItem
                        };

                    entry.changes.Add(change);

                }

                entries.Add(entry); 
            }
        }

        public string type { get; set; }

        public List<NotificationEntry> entries { get; set; }
    }

    public class NotificationEntry
    {
        public NotificationEntry()
        {
            this.changes = new List<NotificationChange>();
        }

        public string id { get; set; }

        public DateTime time { get; set; }

        public List<NotificationChange> changes { get; set; }
    }

    public class NotificationChange
    {
        public string field {get; set;}
        public NotificationValue value {get; set;}
    }

    public class NotificationValue
    {
        public string item { get; set;}
        public string verb { get; set;}
        public string parent_id { get; set;}
        public string sender_id { get; set;}
        public DateTime created_time { get; set;}
    }

}