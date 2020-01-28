using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Globalization;
using Facebook;
using SocialManager.Models;
using SocialManager.Extensions;
using SocialManager.Filters;

namespace SocialManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
           //DateTime testdate =  Facebook.DateTimeConvertor.FromUnixTime("1419045960");
                //<div class="hidden">{&amp;quot;object&amp;quot;:&amp;quot;user&amp;quot;,&amp;quot;entry&amp;quot;:[{&amp;quot;uid&amp;quot;:&amp;quot;10201555820336062&amp;quot;,&amp;quot;id&amp;quot;:&amp;quot;10201555820336062&amp;quot;,&amp;quot;time&amp;quot;:1419045960,&amp;quot;changed_fields&amp;quot;:[&amp;quot;feed&amp;quot;]}]}</div>

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}