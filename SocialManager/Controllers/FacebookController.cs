using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Facebook;
using SocialManager.Models;
using SocialManager.Extensions;
using SocialManager.Filters;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Configuration;

namespace SocialManager.Controllers
{
    [Authorize]
    public class FacebookController : BaseController
    {
        // GET: Facebook
        [FacebookCheckPermission]
        public async Task<ActionResult> Index()
        {
                #region Permission Check
                PermissionRequestViewModel permissionViewModel = base.GetMissingPermissions();
                if (permissionViewModel != null &&
                    permissionViewModel.MissingPermissions.Count > 0)
                {
                    return View("FB_RequestPermission", permissionViewModel);
                }
                #endregion
                var access_token = HttpContext.Items["access_token"].ToString();
                if (!string.IsNullOrEmpty(access_token))
                {
                    var appsecret_proof = access_token.GenerateAppSecretProof();
                    var fb = new FacebookClient(access_token);

                    //Get current user's profile
                    dynamic myInfo = await fb.GetTaskAsync("me?fields=first_name,last_name,link,locale,email,name,birthday,gender,location,bio,age_range".GraphAPICall(appsecret_proof));

                    //get current picture
                    dynamic profileImgResult = await fb.GetTaskAsync("{0}/picture?width=100&height=100&redirect=false".GraphAPICall((string)myInfo.id, appsecret_proof));

                    //Hydrate FacebookProfileViewModel with Graph API results
                    var facebookProfile = DynamicExtension.ToStatic<FacebookProfileViewModel>(myInfo);
                    facebookProfile.ImageURL = profileImgResult.data.url;
                    return View(facebookProfile);
                }
                else
                    throw new HttpException(404, "Missing Access Token");
        }

        public async Task<ActionResult> FB_TaggableFriends()
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();

                var fb = new FacebookClient(access_token);
                dynamic myInfo = await fb.GetTaskAsync("me/taggable_friends".GraphAPICall(appsecret_proof));
                var friendsList = new List<FacebookFriendViewModel>();
                foreach (dynamic friend in myInfo.data)
                {

                    friendsList.Add(DynamicExtension.ToStatic<FacebookFriendViewModel>(friend));
                }

                return PartialView(friendsList);
            }
            else
                throw new HttpException(404, "Missing Access Token");
        }

        public async Task<ActionResult> FB_AdminPages()
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();

                var fb = new FacebookClient(access_token);
                dynamic myPages = await fb.GetTaskAsync(
                    "me/accounts?fields=id, name, link, is_published, likes, talking_about_count"
                    .GraphAPICall(appsecret_proof));
                var pageList = new List<FacebookPageViewModel>();
                foreach (dynamic page in myPages.data)
                {

                    pageList.Add(DynamicExtension.ToStatic<FacebookPageViewModel>(page));
                }

                return PartialView(pageList);
            }
            else
                throw new HttpException(404, "Missing Access Token");
        }


        public async Task<ActionResult> FB_GetFeed()
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();

                var fb = new FacebookClient(access_token);
       
                dynamic myFeed = await fb.GetTaskAsync(
                    ("me/feed?fields=id,from {{id, name, picture{{url}} }},story,picture,link,name,description," +
                    "message,type,created_time,likes,comments")
                    .GraphAPICall(appsecret_proof));
                var postList = new List<FacebookPostViewModel>();
                foreach (dynamic post in myFeed.data)
                {
                    postList.Add(DynamicExtension.ToStatic<FacebookPostViewModel>(post));
                }
                return PartialView(postList);
            }
            else
                throw new HttpException(404, "Missing Access Token");
        }

        #region Testing Action

        public async Task<ActionResult> FB_RevokeAccessToken()
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();

                var fb = new FacebookClient(access_token);
                dynamic myFeed = await fb.DeleteTaskAsync(
                    "me/permissions".GraphAPICall(appsecret_proof));
                return RedirectToAction("Index", "Home");
            }
            else
                throw new HttpException(404, "Missing Access Token");
        }

        public Action FB_TestApiLimit()
        {
            throw new FacebookApiLimitException();
        }

        #endregion


        #region Permission Checking

        //private bool checkPermission()
        //{
        //    bool checkPermission = true;
        //    if (TempData["ProcessingPermissionRequest"] != null)
        //    {
        //        checkPermission = !((bool)TempData["ProcessingPermissionRequest"]);
        //    }
        //    return checkPermission;
        //}

        protected override Dictionary<string, FacebookPermissionRequest> GetRequiredPermissions()
        {
            Dictionary<string, FacebookPermissionRequest> RequiredPermissions =
                new Dictionary<string, FacebookPermissionRequest>();

            RequiredPermissions.Add
            ("read_stream",
                    new FacebookPermissionRequest
                    {
                        name = "View Your Posts",
                        description = "Provides the Social Manager with ability to present a listing of your Facebook Posts, Statuses, and Activities on the Posts Tab of the Profile Page.",
                        permision_scope_value = "read_stream",
                        requested = false
                    }
            );

            RequiredPermissions.Add
            ("manage_pages",
                    new FacebookPermissionRequest
                    {
                        name = "View Your Facebook Pages",
                        description = "Provides the Social Manager with ability to present a listing of Facebook Pages that you administer on the Pages Tab of the Profile Page.",
                        permision_scope_value = "manage_pages",
                        requested = false
                    }
            );

            RequiredPermissions.Add
            ("user_friends",
                    new FacebookPermissionRequest
                    {
                        name = "View Your Friends",
                        description = "Provides the Social Manager with ability to present a listing of your Friends to select for tagging purposes.",
                        permision_scope_value = "user_friends",
                        requested = false
                    }
            );
            return RequiredPermissions;

        }

        [HttpPost]
        public ActionResult RequestPermissions(
            string okButton, 
            string cancelButton, 
            string redirectURI,
            PermissionRequestViewModel acknowledgedPermissionRequest)
        {
            TempData["ProcessingPermissionRequest"] = true;
            if (okButton != null)
            {
                StringBuilder permission_scope = new StringBuilder();
                foreach (FacebookPermissionRequest permission in 
                    acknowledgedPermissionRequest.MissingPermissions.
                    Where(x => x.requested == true))
                {
                    permission_scope.Append(
                        string.Format("{0} ",
                        permission.permision_scope_value));
                }
                if (permission_scope.Length == 0)
                    return Redirect("Index");
                else
                {
                    var rerequestURL = base.AddPermissions(permission_scope.ToString(), redirectURI);
                    return Redirect(rerequestURL);
                }
            }
            else  //if(cancelButton != null)
            {
                return Redirect("Index");
            }
        }      

        #endregion
    }
}