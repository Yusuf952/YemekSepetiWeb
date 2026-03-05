using Microsoft.AspNetCore.Mvc;

namespace YemekSepetiWeb.Controllers
{
    public class HomeController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
