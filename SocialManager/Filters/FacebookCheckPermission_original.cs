using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialManager.Models;
using SocialManager.Controllers;
using System.Web.Routing;

namespace SocialManager.Filters
{
    public class FacebookCheckPermission : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            #region Permission Check
            var myBaseController = (BaseController)filterContext.Controller;
            PermissionRequestViewModel permissionViewModel = myBaseController.GetMissingPermissions();
            if (permissionViewModel != null &&
                permissionViewModel.MissingPermissions.Count > 0)
            {
                //Set redirect URI using current http request controller's index action
                permissionViewModel.redirectURI = myBaseController.Url.Action("Index",
                    filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, null,
                    filterContext.HttpContext.Request.Url.Scheme);

                filterContext.Result = myBaseController.FB_RequestPermission(permissionViewModel);
                return;
            }
            #endregion

            base.OnActionExecuting(filterContext);
        }
    }
}