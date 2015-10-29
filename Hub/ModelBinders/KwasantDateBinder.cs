using System;
using System.Globalization;
using System.Web.Mvc;

namespace Hub.ModelBinders
{
    public class KwasantDateBinder : IModelBinder
    {
        public const string DateTimeFormat = @"MM/dd/yyyy hh:mm tt ""GMT""zzz";
        
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException("controllerContext", "controllerContext is null.");
            if (bindingContext == null)
                throw new ArgumentNullException("bindingContext", "bindingContext is null.");

            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null)
                throw new ArgumentNullException(bindingContext.ModelName);

            var attemptedValue = value.AttemptedValue;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
            
            DateTimeOffset dto;
            var res = DateTimeOffset.TryParseExact(attemptedValue, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dto);
            if (!res)
            {
                res = DateTimeOffset.TryParse(attemptedValue, out dto);
                if (!res)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid datetime string.");
                return null;
                }
            }
            return dto;
        }
    }
}
