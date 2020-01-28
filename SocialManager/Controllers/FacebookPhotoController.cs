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
using System.Dynamic;

namespace SocialManager.Controllers
{
    [Authorize]
    public class FacebookPhotoController : BaseController
    {
        protected override Dictionary<string, FacebookPermissionRequest> GetRequiredPermissions()
        {
            var RequiredPermissions =
                new Dictionary<string, FacebookPermissionRequest> { 
                { "publish_actions", 
                    new FacebookPermissionRequest { 
                        name = "Posting", 
                        description = "Provides the Social Manager with ability to create new albums and photos.", 
                        permision_scope_value = "publish_actions", 
                        requested = false } },
                { "user_photos", 
                    new FacebookPermissionRequest { 
                        name = "Photo Access", 
                        description = "Provides the Social Manager with ability to view and create albums and photos.", 
                        permision_scope_value = "user_photos", 
                        requested = false } }
               };

            return RequiredPermissions;

        }

        // GET: FacebookPhoto
        [FacebookCheckPermission]
        public ActionResult Index()
        {
            return View();
        }
        
        #region Albums Actions
        [HttpPost]
        public async Task<ActionResult> GetAlbums()
        {
            #region Facebook Graph API Retrieve Albums
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();
                var fb = new FacebookClient(access_token);

                #region Get Photo Albums
                dynamic myInfo = await fb.GetTaskAsync(
                        string.Format("me/albums?fields=id,name, count, link," +
                                      "picture, privacy")
                        .GraphAPICall(appsecret_proof), null);
                #endregion

                #region Hydrate results
                //Hydrate FacebookProfileViewModel with Graph API results
                var albumList = new List<FacebookAlbumViewModel>();
                foreach (dynamic album in myInfo.data)
                {
                    albumList.Add(DynamicExtension.ToStatic<FacebookAlbumViewModel>(album));
                }
                #endregion
                return PartialView("FB_GetAlbums_Response", albumList);
            }
            else
                throw new HttpException(404, "Missing Access Token");
            #endregion
        }

        [HttpPost]
        public async Task<ActionResult> CreateAlbum(string NewAlbumName, string Privacy)
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();
                var fb = new FacebookClient(access_token);

                #region Get Photo Albums
                dynamic myInfo = await fb.PostTaskAsync(
                        (string.Format("me/albums?name={0}",
                        NewAlbumName) +
                        "&privacy={{'value':'" + Privacy + "'}}")
                        .GraphAPICall(appsecret_proof), null);
                #endregion

                return null; //on success event formNewAlbum ajax form handles redirecting to the index view
                             //This could return the new album id that we get back from Facebook's graph api
                             //If the new album id is needed for your implementation
            }
            else
                throw new HttpException(404, "Missing Access Token");
        }

        [HttpPost]
        public async Task<ActionResult> ReloadAlbum(string ReloadAlbumId)
        {

            #region Facebook Graph API Retrieve Albums
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();
                var fb = new FacebookClient(access_token);

                #region Get Photo Albums
                dynamic myInfo = await fb.GetTaskAsync(
                        string.Format(
                        "me/albums/{0}?fields=id,name,count,link,can_upload, picture, privacy",
                        ReloadAlbumId)
                        .GraphAPICall(appsecret_proof), null);
                #endregion

                #region Hydrate results
                //Hydrate FacebookProfileViewModel with Graph API results
                FacebookAlbumViewModel album = DynamicExtension.ToStatic<FacebookAlbumViewModel>
                    (myInfo.data[0]);
                #endregion

                return PartialView("FB_Album_Response", album);

            }
            else
                throw new HttpException(404, "Missing Access Token");
            #endregion
        }

        #endregion

        #region Photo Actions
        [HttpPost]
        public async Task<ActionResult> GetPhotos(
                    string AlbumId)
        {
            #region Facebook Graph API Retrieve Albums
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                if (!string.IsNullOrEmpty(AlbumId))
                {
                    var appsecret_proof = access_token.GenerateAppSecretProof();
                    var fb = new FacebookClient(access_token);

                    #region Get Photo Albums
                    dynamic PhotoResults = await fb.GetTaskAsync(
                            (string.Format("{0}/photos", AlbumId)
                            + "?fields=id,picture,name,images{{source}}&limit=25")
                            .GraphAPICall(appsecret_proof));
                    #endregion

                    #region Results
                    var photoList = HydratePhotoList(PhotoResults);
                    ViewBag.ShowGetMoreData = GetMoreDataQuery(PhotoResults, access_token);
                    #endregion

                    return PartialView("FB_GetPhotos_Response", photoList);
                }
                else
                    throw new HttpException(404, "Missing Current Album Id");

            }
            else
                throw new HttpException(404, "Missing Access Token");
            #endregion
        }

        [HttpPost]
        public async Task<ActionResult> GetMorePhotos(string NextPageUri)
        {
            if (!string.IsNullOrEmpty(NextPageUri))
            {


                #region Get More Paged Data
                var access_token = HttpContext.Items["access_token"].ToString();
                if (!string.IsNullOrEmpty(access_token))
                {
                    var appsecret_proof = access_token.GenerateAppSecretProof();
                    var fb = new FacebookClient(access_token);
                    dynamic nextPageResult = await fb.GetTaskAsync(NextPageUri.GraphAPICall(appsecret_proof));

                    #region Results
                    var photoList = HydratePhotoList(nextPageResult);
                    ViewBag.ShowGetMoreData = GetMoreDataQuery(nextPageResult, access_token);
                    #endregion

                    return PartialView("FB_GetPhotos_Response", photoList);
                }
                else
                    throw new HttpException(404, "Missing Access Token");
                #endregion
            }
            else
                return null;
        }


        [HttpPost]
        public async Task<JsonResult> PostVideo(IEnumerable<HttpPostedFileBase> VideosToUpload,
                                                  string VideoDescription)
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            //Get Upload Photo Data
            var UploadedVideo = VideosToUpload.First();
            var video_FBObject = new Facebook.FacebookMediaStream
            {
                FileName = UploadedVideo.FileName,
                ContentType = UploadedVideo.ContentType
            };
            video_FBObject.SetValue(UploadedVideo.InputStream);

            //FB Graph API Parameters
            dynamic parameters = new ExpandoObject();
            parameters.access_token = access_token;
            //If you have enabled requires app secret proof on FB app dashboard for your registerted application
            //parameters.appsecret_proof = access_token.GenerateAppSecretProof();
            parameters.source = video_FBObject;
            if (!string.IsNullOrEmpty(VideoDescription))
                parameters.message = VideoDescription;

            //region FB Graph API Add Photo
            var fb = new FacebookClient();
            dynamic result = await fb.PostTaskAsync(
                  "/me/videos", parameters);

            #region Add Photo Result
            if (result != null &&
                result.id != null)
                if (result.post_id != null)
                    return Json(new { status = "posted" });
                else
                    return Json(new { status = "pending" });
            else
                return Json(new { status = "failed" });
            #endregion

           
        }

        [HttpPost]
        public async Task<JsonResult> PostPhoto(IEnumerable<HttpPostedFileBase> PhotosToUpload,
                                                  string TargetAlbumId,
                                                  string PhotoDescription)
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {

                if (PhotosToUpload != null &&
                    PhotosToUpload.Count() > 0 &&
                    PhotosToUpload.First().HasFile() &&
                    !string.IsNullOrEmpty(TargetAlbumId))
                {
                    #region Get Upload Photo Data
                    var UploadedPhoto = PhotosToUpload.First();
                    var photo_FBObject = new Facebook.FacebookMediaStream
                    {
                        FileName = UploadedPhoto.FileName,
                        ContentType = UploadedPhoto.ContentType
                    };
                    photo_FBObject.SetValue(UploadedPhoto.InputStream);
                    #endregion

                    #region FB Graph API Parameters
                    dynamic parameters = new ExpandoObject();
                    parameters.access_token = access_token;
                    parameters.appsecret_proof = access_token.GenerateAppSecretProof();
                    parameters.source = photo_FBObject;
                    if (!string.IsNullOrEmpty(PhotoDescription))
                        parameters.message = PhotoDescription;
                    #endregion

                    #region FB Graph API Add Photo
                    var fb = new FacebookClient();
                    dynamic result = await fb.PostTaskAsync(
                        string.Format("/{0}/photos", TargetAlbumId), parameters);
                    #endregion

                    #region Add Photo Result
                    if (result != null &&
                        result.id != null)
                        if (result.post_id != null)
                            return Json(new { status = "posted" });
                        else
                            return Json(new { status = "pending" });
                    else
                        return Json(new { status = "failed" });
                    #endregion

                }
                return null;
            }
            else
                throw new HttpException(404, "Missing Access Token");
        }

        [HttpPost]
        public async Task<JsonResult> DeletePhoto(string PhotoId)
        {
            var access_token = HttpContext.Items["access_token"].ToString();
            if (!string.IsNullOrEmpty(access_token))
            {
                var appsecret_proof = access_token.GenerateAppSecretProof();
                var fb = new FacebookClient(access_token);

                #region Delete Photo
                dynamic result = await fb.DeleteTaskAsync(
                        string.Format("/{0}",
                        PhotoId)
                        .GraphAPICall(appsecret_proof));

                return Json(result);
                #endregion
            }
            else
                throw new HttpException(404, "Missing Access Token");
        }

        private List<FacebookPhotoViewModel> HydratePhotoList(dynamic Result)
        {
            var photoList = new List<FacebookPhotoViewModel>();
            foreach (dynamic photo in Result.data)
            {

                photoList.Add(DynamicExtension.ToStatic<FacebookPhotoViewModel>(photo));
            }
            return photoList;
        }

        private string GetMoreDataQuery(dynamic Result, string access_token)
        {
            string NextPageURI = string.Empty;

            if (Result.paging != null &&
                Result.paging.next != null)
                NextPageURI = Result.paging.next;

            return base.GetNextPageQuery(NextPageURI, access_token);
        }

        #endregion
    }
}