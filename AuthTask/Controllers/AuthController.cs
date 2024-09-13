using AuthTask.Models;
using AuthTask.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AuthTask.ViewModels;

namespace AuthTask.Controllers
{
    [Route("auth")]
    public class AuthController(IAuthManager authManager) : BaseController
    {
        [HttpGet("login")]
        public IActionResult LogIn(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login"), AutoValidateAntiforgeryToken]
        public IActionResult LogIn(string? returnUrl, [FromForm]UserLogin user)
        {
            if (!ModelState.IsValid) return View(user);

            var result = authManager.SignIn(user.Email, user.Password);
            if (result.IsFailure)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ModelState.AddModelError(string.Empty, result.Message);
                return View(user);
            }

            return ManageReturnUrl(returnUrl);
        }

        [HttpGet("logout")]
        public IActionResult LogOut()
        {
            authManager.SignOut();

            return ManageReturnUrl(null);
        }

        [HttpGet("register")]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("register"), AutoValidateAntiforgeryToken]
        public IActionResult Register([FromQuery] string? returnUrl,
            [FromForm][Bind("Name,Email,Password")] User user,
            [FromServices]IUserRepository repository)
        {
            if (!ModelState.IsValid || !AreValid(user, repository)) 
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(user);
            }
            var userRes = repository.Create(user);
            if (userRes.IsFailure) return RedirectToError(userRes);

            return RedirectToAction(nameof(LogIn), new { returnUrl });
        }

        [NonAction]
        private bool AreValid(User user, IUserRepository repository)
        {
            var nameRes = repository.NameExists(user.Name);
            var isNameInvalid = nameRes.IsSuccess && nameRes;
            if (isNameInvalid)
                ModelState.AddModelError(nameof(user.Name), "The username is already in use.");
            var emailRes = repository.EmailExists(user.Email);
            var isEmailInvalid = emailRes.IsSuccess && emailRes;
            if (isEmailInvalid)
                ModelState.AddModelError(nameof(user.Email), "The email is already in use.");

            return !(isEmailInvalid && isEmailInvalid);
        }
    }
}
