using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DSE.App.Helpers
{
    public static class Validator
    {
        public static MvcHtmlString MyValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            var spliter = expression.Body.ToString().Split('.');
            string prop = spliter[spliter.Length - 1];
            if(!helper.ViewData.ModelState.IsValidField(prop))
            //var a = helper.ValidationMessageFor(expression).ToHtmlString();
            //if (helper.ValidationMessageFor(expression).ToHtmlString() != "<span class=\"field-validation-valid\" data-valmsg-for=\"UserName\" data-valmsg-replace=\"true\"></span>")
            {
                TagBuilder containerDivBuilder = new TagBuilder("ul");
                containerDivBuilder.AddCssClass("parsley-errors-list filled");

                TagBuilder midDivBuilder = new TagBuilder("li");

                midDivBuilder.InnerHtml = helper.ValidationMessageFor(expression).ToString();

                containerDivBuilder.InnerHtml += midDivBuilder.ToString(TagRenderMode.Normal);

                return MvcHtmlString.Create(containerDivBuilder.ToString(TagRenderMode.Normal));
            }
            return new MvcHtmlString("");
        }
    }
}