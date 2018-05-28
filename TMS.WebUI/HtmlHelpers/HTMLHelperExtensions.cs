using TMS.Domain.Common;
using TMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace TMS.WebApp.HtmlHelpers
{
    public static class HTMLHelperExtensions
    {
        public static string IsSelected(this HtmlHelper html, string controller = null, string action = null, string cssClass = null)
        {

            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller == currentController && action == currentAction ?
                cssClass : String.Empty;
        }

        public static string GenerateStatusHTMLCode(int? state, int taskId, IEnumerable<Event> eventList)
        {
            var result = string.Empty;

            foreach (Event item in eventList)
            {
                result += ReturnAtag(item.ID, taskId, item.Name);
            }

            return result;
        }
        public static string ReturnAtag(int nextEvent, int taskId, string eventName, string className = "")
        {
            return string.Format("<a href='/Worktask/UpdateStatus?worktaskId={0}&nextEvent={1}' class='btn btn-white form-control {2}'><i class='fa fa-angle-double-right'></i> {3}</a>", taskId, nextEvent, className, eventName);
        }

        public static MvcHtmlString CustomEnumDropDownListFor<TModel, TEnum>(
            this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            var items =
                values.Select(
                   value =>
                   new SelectListItem
                   {
                       Text = GetEnumDescription(value),
                       Value = value.ToString(),
                       Selected = value.Equals(metadata.Model)
                   });
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return htmlHelper.DropDownListFor(expression, items, attributes);
        }

        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
        public static string PageClass(this HtmlHelper html)
        {
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            return currentAction;
        }
        public static string GetCatName(this HtmlHelper htmlHelper, int catValue, IEnumerable<SelectListItem> selectList)
        {
            var res = selectList.Where<SelectListItem>(s => s.Value == catValue.ToString()).FirstOrDefault();
            return (res == null ? string.Empty : res.Text);
        }
    }
}