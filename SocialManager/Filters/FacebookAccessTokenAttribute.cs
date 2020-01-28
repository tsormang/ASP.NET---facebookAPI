using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Security.Principal;
using SocialManager.Models;
using SocialManager.Controllers;
using System.Web.Routing;

namespace SocialManager.Filters
{
    public class FacebookAccessTokenAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            ApplicationUserManager _userManager = filterContext.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            if (_userManager != null)
            {
                var User = filterContext.HttpContext.User.Identity.GetUserId();
                if (User != null)
                {
                    var claimsforUser = _userManager.GetClaimsAsync(User);
                    if (claimsforUser != null)
                    {
                        string access_token_value = "_DoesNotExists_";
                        var access_token = claimsforUser.Result.FirstOrDefault(x => x.Type == "FacebookAccessToken");

                        if (access_token != null)
                        {
                            access_token_value = access_token.Value;
                        }

                        if (filterContext.HttpContext.Items.Contains("access_token"))
                            filterContext.HttpContext.Items["access_token"] = access_token_value;
                        else
                            filterContext.HttpContext.Items.Add("access_token", access_token_value);
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }

}