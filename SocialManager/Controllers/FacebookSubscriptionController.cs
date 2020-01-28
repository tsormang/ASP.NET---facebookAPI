using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Dynamic;
using Facebook;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using SocialManager.Models;
using SocialManager.Extensions;

namespace SocialManager.Controllers
{
    [System.Web.Mvc.Authorize]
    public class FacebookSubscriptionController : BaseController
    {
        [HttpGet]
        [ActionName("Index")]
        [AllowAnonymous]
        public ResponseActionResult ValidateSubscription()
        {                    
            string hubMode = Request.QueryString["hub.mode"];
            string hubVerifyToken = Request.QueryString["hub.verify_token"];
            string hubChallenge = Request.QueryString["hub.challenge"];

            Facebook.FacebookClient fb = new FacebookClient();
            fb.SubscriptionVerifyToken = 
                ConfigurationManager.AppSettings["Facebook_Verify_Token"];
            // VerifyGetSubscription will throw exception if verification fails.
            fb.VerifyGetSubscription(hubMode,
                                     hubVerifyToken,
                                     hubChallenge);
            string message = string.Format("Subscription Verification Received - {0}",
                        hubVerifyToken);

            #region Broadcast Message
                GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().
                    Clients.All.
                    addNewNotificationToPage(
                             RenderRazorViewToString("FB_NotificationItem",
                             NotificationMessage.NewMessage("Subscription", message))
                    );
            #endregion
 
            return new ResponseActionResult(hubChallenge);
        }

        [HttpPost]
        [ActionName("Index")]
        [AllowAnonymous]
        public HttpStatusCodeResult ReceiveNotification()
        {
                Facebook.FacebookClient fb = new FacebookClient();
                fb.SetJsonSerializers(null, Deserializer);
                // VerifyPostSubscription will throw exception if verification fails.
                // result is a json object that was sent by Facebook                
                FacebookNotifyViewModel notification = 
                    (FacebookNotifyViewModel)fb.VerifyPostSubscription(
                                    Request.Headers["X-Hub-Signature"],
                                    new StreamReader(Request.InputStream).ReadToEnd(),
                                    typeof(FacebookNotifyViewModel),
                                    ConfigurationManager.AppSettings["Facebook_AppSecret"]);
                #region Broadcast notification         
                GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().
                    Clients.All.
                    addNewNotificationToPage(
                                RenderRazorViewToString("FB_NotificationItem", notification)
                    );
                if (notification.Object == "page")
                {
                    GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().
                        Clients.All.
                        FBPageNotification(notification.entry.First().id);
                }

                #endregion

                return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private FacebookNotifyViewModel Deserializer(string input, Type deserializerType)
        {
            return JsonConvert.DeserializeObject<FacebookNotifyViewModel>(input);
        }

        public ActionResult Manage()
        {

            FacebookAddSubscriptionViewModel notificationmodel = 
                new FacebookAddSubscriptionViewModel(
                                    GetUserSubscriptionFields(),
                                    GetPageSubscriptionFields(),
                                    GetPermissionsSubscriptionFields()
                                    );
            return View(notificationmodel);
        }
 
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            #region Facebook Graph API Retrieve Subscriptions
            var app_token = base.GetAppToken;
            if (!string.IsNullOrEmpty(app_token))
            {
                var fb = new FacebookClient(app_token);
                #region Get Subscriptions
                dynamic result = await fb.GetTaskAsync(
                        string.Format("subscriptions"), null);
                #endregion

                #region Hydrate results
                //Hydrate FacebookSubscriptionViewModel with Graph API results
                var subscriptionList = new List<FacebookSubscriptionViewModel>();
                foreach (dynamic subscription in result.data)
                {
                    subscriptionList.Add(
                        DynamicExtension.ToStatic<FacebookSubscriptionViewModel>(subscription));
                }
                #endregion
                return PartialView("FB_GetSubscriptions_Response", subscriptionList);
            }
            else
                throw new HttpException(404, "Missing App Token");
            #endregion
        }

        [HttpPost]
        public async Task<JsonResult> Add(string subscription, string[] SubscriptionField)
        {
            #region Facebook Graph API Retrieve Subscriptions
            var app_token = GetAppToken;
            if (!string.IsNullOrEmpty(app_token))
            {
                var fb = new FacebookClient(app_token);
                #region Get Subscriptions
                dynamic result = await fb.PostTaskAsync(
                        string.Format("subscriptions?object={0}&callback_url={1}&"+
                                      "verify_token={2}&fields={3}",
                        subscription,
                        ConfigurationManager.AppSettings["Facebook_Subscription_Callback"],
                        ConfigurationManager.AppSettings["Facebook_Verify_Token"],
                        Server.UrlEncode(String.Join(",", SubscriptionField))), 
                        null);
                #endregion

                 return Json(result);
            }
            else
                throw new HttpException(404, "Missing App Token");
            #endregion
        }

        [HttpPost]
        public async Task<JsonResult> Delete(string subscription_to_delete)
        {
            #region Facebook Graph API Retrieve Subscriptions
            var app_token = GetAppToken;
            if (!string.IsNullOrEmpty(app_token))
            {
                var fb = new FacebookClient(app_token);
                #region Delete Subscription
                dynamic result = await fb.DeleteTaskAsync(
                        string.Format("subscriptions?object={0}", 
                        subscription_to_delete));
                #endregion

                return Json(result);
            }
            else
                throw new HttpException(404, "Missing App Token");
            #endregion       
        }
        private string[] GetUserSubscriptionFields()
        {
            return new string[]{"about",
                                "about_me",
                                "activities",
                                "allowed_restrictions",
                                "birthday",
                                "birthday_date",
                                "books",
                                "checkin_deal_claims",
                                "contact_email",
                                "current_location",
                                "education",
                                "education_history",
                                "email",
                                "email_hashes",
                                "events",
                                "family",
                                "feed",
                                "first_name",
                                "friend_request",
                                "has_added_app",
                                "hometown",
                                "hometown_location",
                                "hs_info",
                                "interests",
                                "is_app_user",
                                "is_blocked",
                                "last_name",
                                "likes",
                                "link",
                                "locale",
                                "location",
                                "meeting_for",
                                "meeting_sex",
                                "movies",
                                "music",
                                "name",
                                "notes_count",
                                "online_presence",
                                "photos",
                                "pic",
                                "picture",
                                "pic_https",
                                "pic_with_logo",
                                "pic_big",
                                "pic_big_https",
                                "pic_big_with_logo",
                                "pic_small",
                                "pic_small_https",
                                "pic_small_with_logo",
                                "pic_square",
                                "pic_square_https",
                                "pic_square_with_logo",
                                "political_views",
                                "profile_blurb",
                                "profile_update_time",
                                "profile_url",
                                "proxied_email",
                                "quotes",
                                "relationship_status",
                                "religion",
                                "gender",
                                "sex",
                                "significant_other_id",
                                "statuses",
                                "timezone",
                                "television",
                                "tv",
                                "verified",
                                "website",
                                "work",
                                "work_history",
                                "friends",
                                "platform",
                                "privacy",
                                "blocked",
                                "ip_optout",
                                "notifications",
                                "threads"
                            };
        }

        private string[] GetPageSubscriptionFields()
        {
            return new string[]{"affiliation",
                                "artists_we_like",
                                "attire",
                                "awards",
                                "band_interests",
                                "band_members",
                                "bio",
                                "birthday",
                                "booking_agent",
                                "built",
                                "category",
                                "checkins",
                                "company_overview",
                                "culinary_team",
                                "current_location",
                                "description",
                                "directed_by",
                                "email",
                                "features",
                                "feed",
                                "founded",
                                "general_info",
                                "general_manager",
                                "genre",
                                "hometown",
                                "hours",
                                "influences",
                                "location",
                                "members",
                                "mission",
                                "mpg",
                                "name",
                                "network",
                                "parking",
                                "payment_options",
                                "personal_info",
                                "personal_interests",
                                "phone",
                                "picture",
                                "plot_outline",
                                "press_contact",
                                "price_range",
                                "produced_by",
                                "productlists",
                                "products",
                                "public_transit",
                                "record_label",
                                "release_date",
                                "restaurant_services",
                                "restaurant_specialties",
                                "schedule",
                                "screenplay_by",
                                "season",
                                "starring",
                                "studio",
                                "website",
                                "written_by"
                            };
        }

        private string[] GetPermissionsSubscriptionFields()
        {
            return new string[]{"ads_management",
                                "ads_read",
                                "email",
                                "manage_notifications",
                                "manage_pages",
                                "publish_actions",
                                "read_friendlists",
                                "read_insights",
                                "read_mailbox",
                                "read_page_mailboxes",
                                "read_stream",
                                "rsvp_event",
                                "user_about_me",
                                "user_actions.books",
                                "user_actions.fitness",
                                "user_actions.music",
                                "user_actions.news",
                                "user_actions.video",
                                "user_activities",
                                "user_birthday",
                                "user_education_history",
                                "user_events",
                                "user_friends",
                                "user_games_activity",
                                "user_groups",
                                "user_hometown",
                                "user_interests",
                                "user_likes",
                                "user_location",
                                "user_photos",
                                "user_relationship_details",
                                "user_relationships",
                                "user_religion_politics",
                                "user_status",
                                "user_tagged_places",
                                "user_vidoes",
                                "user_website",
                                "user_work_history"
                            };
        }
    }
}

