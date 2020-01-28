using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Configuration;
using Facebook;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using SocialManager.Filters;
using SocialManager.Models;
using SocialManager.Extensions;
using System.Runtime.Serialization.Json;

using Newtonsoft.Json;
using System.IO;
using System.Web.Routing;
using System.Reflection;
namespace SocialManager.Controllers
{
    [Authorize]
    [FacebookAccessTokenAttribute]
    public class BaseController : Controller
    {
        protected virtual Dictionary<string, FacebookPermissionRequest> GetRequiredPermissions()
        {
            //derived class to provide dictionary of required permissions
            //otherwise, base class implementation will return a null value
            return null;
        }

        public PermissionRequestViewModel GetMissingPermissions()
        {
            bool checkPermission = true;
            if (TempData["ProcessingPermissionRequest"] != null)
            {
                checkPermission = !((bool)TempData["ProcessingPermissionRequest"]);
            }
            if (checkPermission)
            {
                //Check all permission and create list of Missing Permissions
                PermissionRequestViewModel permissionViewModel = new
                    PermissionRequestViewModel();
                permissionViewModel.MissingPermissions =
                    CheckPermissions(
                            GetRequiredPermissions());
                return permissionViewModel;
            }


            return null;
        }

        public ActionResult FB_RequestPermission(PermissionRequestViewModel MissingPermissions)
        {
            return View("FB_RequestPermission", MissingPermissions);
        }


	    protected override void OnException(ExceptionContext filterContext)
	    {
		    if (filterContext.Exception is FacebookApiLimitException)
		    {
		        //Status message banner notifying user to try again later
		        filterContext.ExceptionHandled = true;
		        filterContext.Result = RedirectToAction("Index", "Message",
		            new MessageViewModel
		            {
		                Type="Warning",
		                Message="Facebook Graph API limit reached, Please try again later..."
		            });
		    }
		    else if (filterContext.Exception is FacebookOAuthException)
		        if (HandleAsExpiredToken((FacebookOAuthException)filterContext.Exception))
		        {
		            filterContext.ExceptionHandled = true;
		            filterContext.Result = GetFacebookLoginURL();
		        }
		        else
		        {
		            //redirect to Facebook Custom Error Page
		            filterContext.ExceptionHandled = true;
		            filterContext.Result = RedirectToAction("Index", "Message",
		                new MessageViewModel
		                { 
		                    Type = "Error",
		                    Message = 
		                    string.Format("{0} controller: {1}", 
		                            filterContext.Exception.Source, 
		                            filterContext.Exception.Message)
		                });
		        }
            else if (filterContext.Exception is FacebookApiException)
            {
                //redirect to Facebook Custom Error Page
                filterContext.ExceptionHandled = true;
                filterContext.Result = RedirectToAction("Index", "Message",
                    new MessageViewModel
                    {
                        Type = "Error",
                        Message =
                            string.Format("{0} controller: {1}",
                                    filterContext.Exception.Source,
                                    filterContext.Exception.Message)
                    });
            }
		    else
		        base.OnException(filterContext);
	    }

        private bool HandleAsExpiredToken(FacebookOAuthException OAuth_ex)
        {
            bool _HandleAsExpiredToken = false;
            if (OAuth_ex.ErrorCode == 190) //OAuthException
            {
                switch (OAuth_ex.ErrorSubcode)
                {
                    case 458: //App Not Installed
                    case 459: //User Checkpointed
                    case 460: //Password Changed
                    case 463: //Expired
                    case 464: //Unconfirmed User
                    case 467: //Invalid access token
                        _HandleAsExpiredToken = true;
                        break;
                    default:
                        _HandleAsExpiredToken = false;
                        break;
                }
            }
            else if (OAuth_ex.ErrorCode == 102)
            {
                //API Session. Login status or access token has expired, 
                //been revoked, or is otherwise invalid
                _HandleAsExpiredToken = true;
            }
            else if (OAuth_ex.ErrorCode == 10)
            {
                //API Permission Denied. Permission is either not granted or has been removed - 
                //Handle the missing permissions
                _HandleAsExpiredToken = false;
            }
            else if (OAuth_ex.ErrorCode >= 200 && OAuth_ex.ErrorCode <= 299)
            {
                //API Permission (Multiple values depending on permission). 
                //Permission is either not granted or has been removed - Handle the missing permissions
                _HandleAsExpiredToken = false;
            }
            return _HandleAsExpiredToken;
        }

        private RedirectResult GetFacebookLoginURL()
        {

            if (Session["AccessTokenRetryCount"] == null ||
                (Session["AccessTokenRetryCount"] != null &&
                 Session["AccessTokenRetryCount"].ToString() == "0"))
            {
                Session.Add("AccessTokenRetryCount", "1");

                FacebookClient fb = new FacebookClient();
                fb.AppId = ConfigurationManager.AppSettings["Facebook_AppId"];
                return Redirect(fb.GetLoginUrl(new
                {
                    scope = ConfigurationManager.AppSettings["Facebook_Scope"],
                    redirect_uri = RedirectUri.AbsoluteUri,
                    response_type = "code"
                }).ToString());
            }
            else
            {
                return Redirect(Url.Action("Index", "Message",
                    new MessageViewModel
                    {
                        Type = "Error",
                        Message = "Unable to obtain a valid Facebook Token after multiple attempts please contact support"
                    }));
            }
        }

        protected Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("ExternalCallBack", "Base");
                return uriBuilder.Uri;
            }
        }

        protected string GetNextPageQuery(string NextPageURI, string access_token)
        {
            string ReturnNextPageURI = NextPageURI
                    .Replace("https://graph.facebook.com/v2.1/", "")
                    .Replace(string.Format("&access_token={0}", access_token), "");


            return ReturnNextPageURI;
        }

        public async Task<ActionResult> ExternalCallBack(string code)
        {
            //Callback return from Facebook will include a unique login encrypted code
            //for this user's login with our application id
            //that we can use to obtain a new access token
            FacebookClient fb = new FacebookClient();

            //Exchange encrypted login code for an access_token
            dynamic newTokenResult = await fb.GetTaskAsync(
                                        string.Format("oauth/access_token?client_id={0}&client_secret={1}&redirect_uri={2}&code={3}",
                                        ConfigurationManager.AppSettings["Facebook_AppId"],
                                        ConfigurationManager.AppSettings["Facebook_AppSecret"],
                                        Url.Encode(RedirectUri.AbsoluteUri),
                                        code));
            ApplicationUserManager UserManager = HttpContext.GetOwinContext().
                GetUserManager<ApplicationUserManager>();
            if (UserManager != null)
            {
                // Retrieve the existing claims for the user and add the FacebookAccessTokenClaim 
                var userId = HttpContext.User.Identity.GetUserId();

                IList<Claim> currentClaims = await UserManager.GetClaimsAsync(userId);

                //check to see if a claim already exists for FacebookAccessToken
                Claim OldFacebookAccessTokenClaim = currentClaims.
                    FirstOrDefault(x => x.Type == "FacebookAccessToken");

                //Create new FacebookAccessToken claim
                Claim newFacebookAccessTokenClaim = new Claim("FacebookAccessToken", 
                    newTokenResult.access_token);
                if (OldFacebookAccessTokenClaim == null)
                {
                    //Add new FacebookAccessToken Claim
                    await UserManager.AddClaimAsync(userId, newFacebookAccessTokenClaim);
                }
                else
                {
                    //Remove the existing FacebookAccessToken Claim
                    await UserManager.RemoveClaimAsync(userId, OldFacebookAccessTokenClaim);
                    //Add new FacebookAccessToken Claim
                    await UserManager.AddClaimAsync(userId, newFacebookAccessTokenClaim);
                }
                Session.Add("AccessTokenRetryCount", "0");
            }

            return RedirectToAction("Index", "Facebook");
        }

        protected List<FacebookPermissionRequest> CheckPermissions(
			Dictionary<string, FacebookPermissionRequest> RequiredPermissions)
		{
	    var access_token = HttpContext.Items["access_token"].ToString();
	    if (!string.IsNullOrEmpty(access_token))
	    {
			    var appsecret_proof = access_token.GenerateAppSecretProof();
			    var fb = new FacebookClient(access_token);
				
		    IEnumerable<FacebookPermissionRequest> MissingPermissions = 
			    new List<FacebookPermissionRequest>();  //initialize to an empty list
		    if (RequiredPermissions != null &&
                RequiredPermissions.Count > 0)
		    {
			    //create an array of Facebook Batch Parameters based on list of RequiredPermission
			    FacebookBatchParameter[] fbBatchParameters = 
				    new FacebookBatchParameter[RequiredPermissions.Values.Count()];
			    int idx = 0;
			    foreach (FacebookPermissionRequest required_permssion in 
                    RequiredPermissions.Values)
			    {
				
				    fbBatchParameters[idx] = new FacebookBatchParameter
				    {
				        HttpMethod = HttpMethod.Get,
				        Path = string.Format("{0}{1}",
				                                "me/permissions/",
				                                required_permssion.permision_scope_value)
				                .GraphAPICall(appsecret_proof)
				    };
				    required_permssion.granted = false; //initalize all granted indicators to false for each required permission
				    idx++;
			    }
			    dynamic permission_Batchresult = fb.Batch(
				    fbBatchParameters
			    );
				
			    if (permission_Batchresult != null)
			    {
				    List<PermissionResults> result = JsonConvert.
                        DeserializeObject<List<PermissionResults>>
                        (permission_Batchresult.ToString());
				
				    foreach (FacebookPermissionModel permissionResult in 
                        result.SelectMany(x=>x.data).Where(y=>y.status=="granted"))
				    {
				        RequiredPermissions[permissionResult.permission].granted = true;
				    }
				    MissingPermissions = RequiredPermissions.Values.
                        Where(p => p.granted == false);
			    }
		    }
		    return MissingPermissions.ToList();
        }
	    else
			    throw new HttpException(404, "Missing Access Token");
	    }
        
        protected string AddPermissions(string permission, string redirectURI)
        {
            FacebookClient fb = new FacebookClient();
            fb.AppId = ConfigurationManager.AppSettings["Facebook_AppId"];
            return fb.GetLoginUrl(new
            {
                scope = permission,
                redirect_uri =redirectURI,     //RedirectUri.AbsoluteUri,
                response_type = "code",
                auth_type = "rerequest"

            }).ToString();
        }

        
        protected string GetAppToken
        {
            get
            {
                var _result = RequestAppToken();
                return _result;
            }
        }       
 
        private string RequestAppToken()
        {
            FacebookClient fb = new FacebookClient();
            dynamic appTokenResult = fb.Get(
                         string.Format(
                              "oauth/access_token?client_id={0}&client_secret={1}" +
                              "&grant_type=client_credentials",
                              ConfigurationManager.AppSettings["Facebook_AppId"],
                              ConfigurationManager.AppSettings["Facebook_AppSecret"]));
            if (appTokenResult != null && appTokenResult.access_token != null)
                return appTokenResult.access_token;
            else
                return string.Empty;
        }


        internal string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                if (ControllerContext == null)
                {
                    TextWriter wr = new StreamWriter(new MemoryStream());
                    var httpcontext = new HttpContext(
                        new HttpRequest("", 
                            "http://" + Assembly.GetExecutingAssembly().GetName().Name, ""), 
                            new HttpResponse(wr));
                    var context = new HttpContextWrapper(httpcontext);
                    var routeData = new RouteData();
                    routeData.Values.Add("controller","FacebookSubscription");
                    ControllerContext = new ControllerContext(
                        new RequestContext((HttpContextBase)context,routeData), 
                            new EmptyController());
                }
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                     viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }


    }

    public class EmptyController : ControllerBase
    {
        protected override void ExecuteCore()
        {

        }
    }
}