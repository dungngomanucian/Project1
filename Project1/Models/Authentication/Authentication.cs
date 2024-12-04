using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Project1.Models.Authentication
{
    public class Authentication:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("Email") == null)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Controller", "Login" },
                        {"Action", "Index" }
                    });
            }
        }
    }
}
