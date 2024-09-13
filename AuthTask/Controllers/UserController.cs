using AuthTask.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthTask.ViewModels;
using AuthTask.Shared;
using AuthTask.Models;

namespace AuthTask.Controllers
{
    [Authorize]
    [Route("users")]
    public class UserController(IUserRepository repository) : BaseController
    {
        [HttpGet("~/")]
        public IActionResult Index(int page = 1, int size = 10)
        {
            var usersRes = repository.GetUsers(page, size);
            if (usersRes.IsFailure) return RedirectToError(usersRes);
            var modelResult = GetUsersPaging(page, size, usersRes.Value);
            if (modelResult.IsFailure) return RedirectToError(modelResult);
            return View(modelResult.Value);
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
        private Result<ShowPaging<User>> GetUsersPaging(int page, int size, IEnumerable<User> items)
        {
            var totalRes = repository.GetTotalUsers();
            if (totalRes.IsFailure) 
                return Result.Failure<ShowPaging<User>>(totalRes.Message, totalRes.StatusCode);

            return Result.Success(new ShowPaging<User>
            {
                PageInfo = new()
                {
                    CurrentPage = page,
                    TotalItems = totalRes,
                    ItemsPerPage = size
                },
                DisplayResult = items
            });
        }
    }
}
