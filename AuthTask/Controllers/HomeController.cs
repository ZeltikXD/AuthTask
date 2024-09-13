﻿using Microsoft.AspNetCore.Mvc;

namespace AuthTask.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        [HttpGet("error")]
        public IActionResult Error(string message)
        {
            return View("Error", new { Message = message });
        }
    }
}