using CampusConnect.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CampusConnect.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return View();

        }           

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
