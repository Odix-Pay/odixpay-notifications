using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using OdixPay.Notifications.Application.Exceptions;
using OdixPay.Notifications.Contracts.Resources.LocalizationResources;

namespace OdixPay.Notifications.API.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly IStringLocalizer<SharedResource> _IStringLocalizer;

        public ValidateModelAttribute(IStringLocalizer<SharedResource> StringLocalizer)
        {
            _IStringLocalizer = StringLocalizer;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Any() == true)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value.Errors
                            .Select(er =>
                            {
                                var message = er.ErrorMessage;
                                var localized = _IStringLocalizer[message];
                                return localized.ResourceNotFound ? message : localized.Value;
                            })
                            .ToArray()
                    );

                throw new ValidationException(_IStringLocalizer, errors);
            }

            base.OnActionExecuting(context);
        }
    }
}
