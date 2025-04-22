using FashionShop.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FashionShop.Controllers
{
    /// <summary>
    /// Base controller for all admin controllers with shared functionality
    /// </summary>
    public abstract class AdminBaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if user is admin before executing any action
            if (!AdminHelpers.IsAdmin(HttpContext.Session))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}