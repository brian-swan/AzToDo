using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzTodo.Models;
using Microsoft.AspNetCore.Authorization;

namespace AzTodo.Controllers
{
	[AllowAnonymous]
	public class HomeController : Controller
    {
        public IActionResult Index()
        {
			return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "A simple ToDo list app.";

            return View();
        }
		
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
