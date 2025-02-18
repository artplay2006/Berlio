using Microsoft.AspNetCore.Mvc;

namespace BerlioWeb.Controllers
{
    public class About : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
