using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialManager.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime ToLocalizedTime(this DateTime inputdatetime, 
                                               string timeZoneName)
        {
            return TimeZoneInfo.ConvertTime(
                    inputdatetime.ToUniversalTime(),
                    TimeZoneInfo.FindSystemTimeZoneById(timeZoneName));
        }
    }
}