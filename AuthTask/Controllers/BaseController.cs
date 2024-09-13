using AuthTask.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace AuthTask.Controllers
{
    public abstract class BaseController : Controller
    {
        [NonAction]
        protected RedirectResult ManageReturnUrl(string? returnUrl)
            => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/");

        [NonAction]
        protected ObjectResult CustomResponse(int statusCode, [ActionResultObjectValue] object? value)
            => new(value) { StatusCode = statusCode };

        [NonAction]
        protected RedirectToActionResult RedirectToError(Result result)
            => RedirectToAction("Error", "Home", new { message = result.Message, statusCode = result.StatusCode });

        [NonAction]
        protected Guid GetCurrentUserId()
        {
            var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            return Guid.TryParse(strId, out var id) ? id : Guid.Empty;
        }
    }
}