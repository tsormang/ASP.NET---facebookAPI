using Facebook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;


namespace SocialManager.Extensions
{


    public static class FacebookHtmlExtensions
    {
        public static string GetPropertyPath<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            Match match = Regex.Match(property.ToString(), @"^[^\.]+\.([^\(\)]+)$");
            return match.Groups[1].Value;
        }

        public static MvcHtmlString FB_AgeRangeFor<TEntity>(this HtmlHelper html, TEntity model, Expression<Func<TEntity, JsonObject>> property, object htmlAttributes)
        {

            JsonObject age_range = property.Compile().Invoke(model);

            //here we can format our age range value
            string age_rangeFormatted = string.Empty;
            if (age_range != null)
            {
                if (age_range.Keys.Contains("min"))
                {
                    age_rangeFormatted = age_range["min"].ToString();
                }
                if (age_range.Keys.Contains("min") &&
                    age_range.Keys.Contains("max"))
                {
                    age_rangeFormatted = age_rangeFormatted + " to " + age_range["min"].ToString();
                }
                else if (age_range.Keys.Contains("max"))
                {
                    age_rangeFormatted = age_range["max"].ToString();
                }

            }
            var name = GetPropertyPath(property);

            return html.TextBox(name, age_rangeFormatted, htmlAttributes);
        }
    }

    public static class InputHelper
    {
        public static IHtmlString CheckboxListItem(this HtmlHelper helper, string name, string value )
        {
            string html = @"<a href=""#"" class=""list-group-item""><div class=""checkbox""><label><input type=""checkbox"" name=""{0}"" id=""{0}.{1}"" value=""{1}"" />{1}</label></div></a>";
            return helper.Raw(string.Format(html, name, value));
        }
    }
}