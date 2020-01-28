using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialManager.Controllers
{
    public class ResponseActionResult : ActionResult
    {
        private string _hubChallenge = string.Empty;

        public ResponseActionResult(string hubChallenge)
        {
            _hubChallenge = hubChallenge;
        }
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Write(_hubChallenge);
        }

    }
}