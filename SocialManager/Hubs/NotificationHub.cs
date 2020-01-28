using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using SocialManager.Controllers;
using SocialManager.Models;
using SocialManager.Extensions;

namespace SocialManager
{
    public class NotificationHub : Hub
    {
        public string Activate()
        {
            return new BaseController().RenderRazorViewToString(
                       "FB_NotificationItem",
                       NotificationMessage.
                       NewMessage("Receiving Notifications"));

        }
    }
}