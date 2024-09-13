using AuthTask.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AuthTask.Controllers
{
    [Authorize]
    [Route("users")]
    public class UserController(IUserRepository repository) : Controller
    {
        [HttpGet("~/")]
        public IActionResult Index(int page = 1, int size = 10)
        {
            var usersRes = repository.GetUsers(page, size);
            if (usersRes.IsFailure) return RedirectToAction("Error", "Home", new { message = usersRes.Message, statusCode = usersRes.StatusCode });

            return View(usersRes.Value);
        }

        [HttpPut("change-status/{id:guid}")]
        [Produces("application/json")]
        public IActionResult ChangeStatus(Guid id, bool newStatus)
        {
            var changeRes = repository.ChangeActiveStatus(id, newStatus);
            if (changeRes.IsFailure) return CustomResponse(changeRes.StatusCode, new { changeRes.Message });
            if (!newStatus && id == GetCurrentUserId()) return RedirectToAction("LogOut", "Auth");
            return Ok();
        }

        [HttpDelete("delete/{id:guid}")]
        [Produces("application/json")]
        public IActionResult Delete(Guid id)
        {
            var result = repository.Delete(id);
            if (result.IsFailure) return CustomResponse(result.StatusCode, new { result.Message });
            if (id == GetCurrentUserId()) return RedirectToAction("LogOut", "Auth");
            return Ok();
        }

        [NonAction]
        private static ObjectResult CustomResponse(int statusCode, [ActionResultObjectValue]object? value)
            => new(value) { StatusCode = statusCode };

        [NonAction]
        private Guid GetCurrentUserId()
        {
            var strId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            return Guid.TryParse(strId, out var id) ? id : Guid.Empty;
        }
    }
}
