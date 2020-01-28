using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialManager.Models;
using SocialManager.Extensions;
using Facebook;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using SocialManager.Filters;


namespace SocialManager.Controllers
{
    [Authorize]
    public class FacebookPostController : BaseController
    {
        // GET: FacebookPost
        [FacebookCheckPermission]
        public ActionResult Index()
        {
            #region Permission Check
            PermissionRequestViewModel permissionViewModel = base.GetMissingPermissions();
            if (permissionViewModel != null &&
                permissionViewModel.MissingPermissions.Count > 0)
            {
                return View("FB_RequestPermission", permissionViewModel);
            }
            #endregion

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> PostUserFeed(
            string Message,
            string Link,
            string Privacy,
            IList<string> FriendList)
        {
            #region Parameter Handling
            if (string.IsNullOrEmpty(Message))
                return Json("Cannot post a status without a message");
            //Default the Privacy to SELF if missing
            if (string.IsNullOrEmpty(Privacy))
                Privacy = "SELF";

            string linkParameter = string.Empty;
            if (!string.IsNullOrEmpty(Link))
            {
                linkParameter = string.Format("&link={0}", Link);
            }

            string FriendTagList = string.Empty;
            if (FriendList != null)
            {
                FriendTagList = string.Format("&place=155021662189&tags={0}",
                                string.Join(",", FriendList));
            }
            #endregion

            #region Facebook Graph API Post to User Feed
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();
                var fb = new FacebookClient(access_token);

                #region Post
                dynamic myInfo = await fb.PostTaskAsync(
                    (string.Format("me/feed?message={0}{1}{2}",
                                    Message,
                                    linkParameter,
                                    FriendTagList) +
                                    "&privacy={{'value':'" + Privacy + "'}}").GraphAPICall(appsecret_proof), null);
                #endregion

                return Json("Status update has been succcessfully posted.");

            }
            else
                throw new HttpException(404, "Missing Access Token");
            #endregion
        }


        protected override Dictionary<string, FacebookPermissionRequest> GetRequiredPermissions()
        {
            Dictionary<string, FacebookPermissionRequest> RequiredPermissions =
                new Dictionary<string, FacebookPermissionRequest>();

            RequiredPermissions.Add
            ("publish_actions",
                    new FacebookPermissionRequest
                    {
                        name = "Posting",
                        description = "Provides the Social Manager with ability to add a new post to your Facebook news feed, page feed, event feed or group feed.",
                        permision_scope_value = "publish_actions",
                        requested = false
                    }
            );

            RequiredPermissions.Add
            ("user_friends",
                    new FacebookPermissionRequest
                    {
                        name = "View Your Friends",
                        description = "Provides the Social Manager with ability to present a listing of your Friends to select for tagging with your post.",
                        permision_scope_value = "user_friends",
                        requested = false
                    }
            );
            return RequiredPermissions;

        }

    }
}