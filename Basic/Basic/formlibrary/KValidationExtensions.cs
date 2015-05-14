// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc.Properties;
using System.Web.Routing;

namespace System.Web.Mvc.Html
{
    public static class KValidationExtensions
    {
        private const string HiddenListItem = @"<li style=""display:none""></li>";
        private static string _resourceClassKey;

        public static string ResourceClassKey
        {
            get { return _resourceClassKey ?? String.Empty; }
            set { _resourceClassKey = value; }
        }

        private static FieldValidationMetadata ApplyFieldValidationMetadata(HtmlHelper htmlHelper, ModelMetadata modelMetadata, string modelName)
        {
            FormContext formContext = htmlHelper.ViewContext.FormContext;
            FieldValidationMetadata fieldMetadata = formContext.GetValidationMetadataForField(modelName, true /* createIfNotFound */);

            // write rules to context object
            IEnumerable<ModelValidator> validators = ModelValidatorProviders.Providers.GetValidators(modelMetadata, htmlHelper.ViewContext);
            foreach (ModelClientValidationRule rule in validators.SelectMany(v => v.GetClientValidationRules()))
            {
                fieldMetadata.ValidationRules.Add(rule);
            }

            return fieldMetadata;
        }

        private static string GetInvalidPropertyValueResource(HttpContextBase httpContext)
        {
            string resourceValue = null;
            if (!String.IsNullOrEmpty(ResourceClassKey) && (httpContext != null))
            {
                // If the user specified a ResourceClassKey try to load the resource they specified.
                // If the class key is invalid, an exception will be thrown.
                // If the class key is valid but the resource is not found, it returns null, in which
                // case it will fall back to the MVC default error message.
                resourceValue = httpContext.GetGlobalResourceObject(ResourceClassKey, "InvalidPropertyValue", CultureInfo.CurrentUICulture) as string;
            }
            return resourceValue;
        }

        private static string GetUserErrorMessageOrDefault(HttpContextBase httpContext, ModelError error, ModelState modelState)
        {
            if (!String.IsNullOrEmpty(error.ErrorMessage))
            {
                return error.ErrorMessage;
            }
            if (modelState == null)
            {
                return null;
            }

            string attemptedValue = (modelState.Value != null) ? modelState.Value.AttemptedValue : null;
            return String.Format(CultureInfo.CurrentCulture, GetInvalidPropertyValueResource(httpContext), attemptedValue);
        }

        private static void ValidateHelper(HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression)
        {
            /*
            FormContext formContext = htmlHelper.ViewContext.GetFormContextForClientValidation();
            if (formContext == null || htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled)
            {
                return; 
            }

            string modelName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
            ApplyFieldValidationMetadata(htmlHelper, modelMetadata, modelName);
        
             */
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString KValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return KValidationMessageFor(htmlHelper, expression, null /* validationMessage */, new RouteValueDictionary());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString KValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string validationMessage)
        {
            return KValidationMessageFor(htmlHelper, expression, validationMessage, new RouteValueDictionary());
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString KValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string validationMessage, object htmlAttributes)
        {
            return KValidationMessageFor(htmlHelper, expression, validationMessage, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString KValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string validationMessage, IDictionary<string, object> htmlAttributes)
        {
            return ValidationMessageHelper(htmlHelper,
                                           ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData),
                                           ExpressionHelper.GetExpressionText(expression),
                                           validationMessage,
                                           htmlAttributes);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Normalization to lowercase is a common requirement for JavaScript and HTML values")]
        private static MvcHtmlString ValidationMessageHelper(this HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression, string validationMessage, IDictionary<string, object> htmlAttributes)
        {
            string modelName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
            FormContext formContext = htmlHelper.ViewContext.FormContext;

            if (!htmlHelper.ViewData.ModelState.ContainsKey(modelName) && formContext == null)
            {
                return null;
            }

            ModelState modelState = htmlHelper.ViewData.ModelState[modelName];
            ModelErrorCollection modelErrors = (modelState == null) ? null : modelState.Errors;
            ModelError modelError = (((modelErrors == null) || (modelErrors.Count == 0)) ? null : modelErrors.FirstOrDefault(m => !String.IsNullOrEmpty(m.ErrorMessage)) ?? modelErrors[0]);

            if (modelError == null && formContext == null)
            {
                return null;
            }

            TagBuilder builder = new TagBuilder("span");
            builder.MergeAttributes(htmlAttributes);
            builder.AddCssClass((modelError != null) ? HtmlHelper.ValidationMessageCssClassName : HtmlHelper.ValidationMessageValidCssClassName);

            if (!String.IsNullOrEmpty(validationMessage))
            {
                builder.SetInnerText(validationMessage);
            }
            else if (modelError != null)
            {
                builder.SetInnerText(GetUserErrorMessageOrDefault(htmlHelper.ViewContext.HttpContext, modelError, modelState));
            }

            if (formContext != null)
            {
                bool replaceValidationMessageContents = String.IsNullOrEmpty(validationMessage);

                if (htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    builder.MergeAttribute("data-valmsg-for", modelName);
                    builder.MergeAttribute("data-valmsg-replace", replaceValidationMessageContents.ToString().ToLowerInvariant());
                }
                else
                {
                    FieldValidationMetadata fieldMetadata = ApplyFieldValidationMetadata(htmlHelper, modelMetadata, modelName);
                    // rules will already have been written to the metadata object
                    fieldMetadata.ReplaceValidationMessageContents = replaceValidationMessageContents; // only replace contents if no explicit message was specified

                    // client validation always requires an ID
                    builder.GenerateId(modelName + "_validationMessage");
                    fieldMetadata.ValidationMessageId = builder.Attributes["id"];
                }
            }

            return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal)); ;
        }

        public static Boolean ValidationStatus<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return ValidationStatusHelper(htmlHelper,
                                           ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData),
                                           ExpressionHelper.GetExpressionText(expression)
                                           );
        }

        private static Boolean ValidationStatusHelper(this HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression)
        {
            string modelName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
            FormContext formContext = htmlHelper.ViewContext.FormContext;

            if (!htmlHelper.ViewData.ModelState.ContainsKey(modelName) && formContext == null)
            {
                return true;
            }

            ModelState modelState = htmlHelper.ViewData.ModelState[modelName];
            ModelErrorCollection modelErrors = (modelState == null) ? null : modelState.Errors;
            ModelError modelError = (((modelErrors == null) || (modelErrors.Count == 0)) ? null : modelErrors.FirstOrDefault(m => !String.IsNullOrEmpty(m.ErrorMessage)) ?? modelErrors[0]);

            if (modelError == null && formContext == null)
            {
                return true;
            }

            if (modelError != null)
            {
                return false;
            }


            return true;
        }

    }
}
