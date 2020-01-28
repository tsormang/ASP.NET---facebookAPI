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
    public class FacebookSearchController : BaseController
    {
        // GET: FacebookSearch
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [FacebookCheckPermission]
        public async Task<ActionResult> Find(
            string QueryValue,
            string SearchBy,
            string SearchCenterCoordinates)
        {
            #region Permission Check
            PermissionRequestViewModel permissionViewModel = base.GetMissingPermissions();
            if (permissionViewModel != null &&
                permissionViewModel.MissingPermissions.Count > 0)
            {
                return View("FB_RequestPermission", permissionViewModel);
            }
            #endregion

            #region Facebook Graph API Search call
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();
                var fb = new FacebookClient(access_token);

                #region Search For Place
                string _searchCenterParam = string.Empty;
                if (SearchBy == "place" &&
                    !string.IsNullOrEmpty(SearchCenterCoordinates))
                    _searchCenterParam = string.Format("&center={0}&distance={1}",
                        SearchCenterCoordinates,
                        ConfigurationManager.AppSettings["Facebook_Distance_Meters"].ToString());
                #endregion

                #region Search Facebook Graph Query call
                dynamic myInfo = await fb.GetTaskAsync(
                    (string.Format("search?q={0}&type={1}{2}&limit=50",
                                    QueryValue,
                                    SearchBy,
                                    _searchCenterParam) +
                        "&fields=name,id,picture.type(large).width(100).height(100){{url}}")
                        .GraphAPICall(appsecret_proof));
                #endregion

                #region Hydrate results
                //Hydrate FacebookProfileViewModel with Graph API results
                var userList = new List<FacebookFriendViewModel>();
                foreach (dynamic friend in myInfo.data)
                {

                    userList.Add(DynamicExtension.ToStatic<FacebookFriendViewModel>(friend));
                }
                

                #endregion

                #region Paging Results
                string NextPageURI = string.Empty;

                if (myInfo.paging != null &&
                    myInfo.paging.next != null)
                    NextPageURI = myInfo.paging.next;

                ViewBag.ShowGetMoreData = GetNextPageQuery(NextPageURI, access_token);
                #endregion

                return PartialView("FindResults", userList);

            }
            else
                throw new HttpException(404, "Missing Access Token");
            #endregion
        }
       
        
        [HttpPost]
        [FacebookCheckPermission]
        public async Task<ActionResult> GetMoreData(string NextPageUri)
        {
            if (!string.IsNullOrEmpty(NextPageUri))
            {
                #region Permission Check
                PermissionRequestViewModel permissionViewModel = base.GetMissingPermissions();
                if (permissionViewModel != null &&
                    permissionViewModel.MissingPermissions.Count > 0)
                {
                    return View("FB_RequestPermission", permissionViewModel);
                }
                #endregion

                #region Get More Paged Data
                var access_token = HttpContext.Items["access_token"].ToString();
                if (!string.IsNullOrEmpty(access_token))
                {
                    var appsecret_proof = access_token.GenerateAppSecretProof();
                    var fb = new FacebookClient(access_token);
                    dynamic nextPageResult = await fb.GetTaskAsync(NextPageUri.GraphAPICall(appsecret_proof));

                    #region Hydrate results
                    //Hydrate FacebookProfileViewModel with Graph API results
                    var userList = new List<FacebookFriendViewModel>();
                    foreach (dynamic friend in nextPageResult.data)
                    {

                        userList.Add(DynamicExtension.ToStatic<FacebookFriendViewModel>(friend));
                    }


                    #endregion

                    #region Paging Results
                    string NextPageURI = string.Empty;

                    if (nextPageResult.paging != null &&
                        nextPageResult.paging.next != null)
                        NextPageURI = nextPageResult.paging.next;

                    ViewBag.ShowGetMoreData = GetNextPageQuery(NextPageURI, access_token);
                    #endregion

                    return PartialView("FindResults", userList);
                }
                else
                    throw new HttpException(404, "Missing Access Token");
                #endregion
            }
            else
                return null;
        }

        [HttpPost]
        public ActionResult GeoCode(string LocationSearch)
        {

            WebRequest request = WebRequest.Create(string.Format(
                "http://dev.virtualearth.net/REST/v1/Locations?query={0}&output=json&key={1}",
                Server.UrlEncode(LocationSearch),
                ConfigurationManager.AppSettings["BingMapKey"].ToString()));
            request.Method = WebRequestMethods.Http.Get;

            WebResponse response = request.GetResponse();

            StreamReader sr = new StreamReader(response.GetResponseStream());
            string _recievBuffer = sr.ReadToEnd();
            dynamic geocodeResults = JsonConvert.DeserializeObject(_recievBuffer);

            #region Hydrate Results
            if (geocodeResults != null &&
                geocodeResults.resourceSets != null &&
                geocodeResults.resourceSets[0] != null &&
                geocodeResults.resourceSets[0].resources != null)
            {
                //Hydrate GeoCodeLocationViewModel with Geo Code results
                var locationList = new List<GeoCodeLocationViewModel>();
                foreach (dynamic resource in geocodeResults.resourceSets[0].resources)
                {
                    locationList.Add(new GeoCodeLocationViewModel()
                    {
                        Name = resource.name.Value,
                        Coordinates = string.Format("{0},{1}",
                        resource.point.coordinates[0].ToString(),
                        resource.point.coordinates[1].ToString())
                    });
                }
            #endregion
                return PartialView("GeoCodeResults", locationList);
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid GeoCode Request");

        }

        protected override Dictionary<string, FacebookPermissionRequest> GetRequiredPermissions()
        {
            Dictionary<string, FacebookPermissionRequest> RequiredPermissions =
                new Dictionary<string, FacebookPermissionRequest>();

            RequiredPermissions.Add
            ("public_profile",
                    new FacebookPermissionRequest
                    {
                        name = "Basic Searching",
                        description = "Provides the Social Manager with ability to search for people, pages, events, groups or places.",
                        permision_scope_value = "public_profile",
                        requested = false
                    }
            );
            return RequiredPermissions;

        }

    }



}