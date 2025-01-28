using Microsoft.AspNetCore.Mvc;

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