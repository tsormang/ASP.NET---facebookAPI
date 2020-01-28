using SocialManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialManager.Extensions
{
    public static class NotificationMessage
    {
        public static FacebookNotifyViewModel NewMessage(string title)
        {
            return NewMessage(title, null);
        }

        public static FacebookNotifyViewModel NewMessage(string title, string message)
        {
            FacebookNotifyViewModel subscriptionNotice = new FacebookNotifyViewModel
            {
                Object = title
            };
            subscriptionNotice.entry = new List<FacebookNotify_Entry>();
            subscriptionNotice.entry.Add(new FacebookNotify_Entry
            {
                id = "",
                time = Facebook.DateTimeConvertor.ToUnixTime(DateTime.Now)
            });
            if (!string.IsNullOrEmpty(message))
            {
                subscriptionNotice.entry[0].changes = new List<FacebookNotify_Change>();
                subscriptionNotice.entry[0].changes.Add(new FacebookNotify_Change
                {
                    field = message
                });
            }

            return subscriptionNotice;
        }
    }
}