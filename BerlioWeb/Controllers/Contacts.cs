using Microsoft.AspNetCore.Mvc;

namespace BerlioWeb.Controllers
{
    public class Contacts : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
