using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialManager.Extensions
{
    public static class DoubleExtension
    {
        public static DateTime ConvertToLocalTime(this double unixtime)
        {
          return Facebook.DateTimeConvertor
                    .FromUnixTime(unixtime)
                    .ToLocalizedTime("Eastern Standard Time");
        }
    }
}