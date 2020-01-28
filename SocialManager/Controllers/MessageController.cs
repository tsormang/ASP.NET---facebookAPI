using SocialManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialManager.Controllers
{
    public class MessageController : Controller
    {
        // GET: Error
        public ActionResult Index(MessageViewModel _messageViewModel)
        {
            return View(_messageViewModel);
        }

    }
}
