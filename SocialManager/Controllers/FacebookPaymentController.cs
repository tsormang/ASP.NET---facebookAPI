using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Facebook;
using Microsoft.AspNet.Facebook.Client;
using System.Configuration;

namespace SocialManager.Controllers
{
   
    public class FacebookPaymentController : Controller
    {
        #region Product Object
        [AllowAnonymous]
        public ActionResult Product()
        {
            return View();
        }
        #endregion

        #region Canvas Page
        [FacebookAuthorize()]
        public ActionResult Index(FacebookContext context)
        {
            ViewBag.AppId = context.Configuration.AppId;
            return View();
        }
        #endregion
    }

}