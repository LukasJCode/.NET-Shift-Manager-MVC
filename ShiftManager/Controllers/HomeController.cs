using ShiftManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ShiftManager.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {}

        public IActionResult Index()
        {
            return View();
        }
    }
}