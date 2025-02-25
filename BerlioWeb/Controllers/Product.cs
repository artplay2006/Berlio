using Microsoft.AspNetCore.Mvc;

namespace BerlioWeb.Controllers
{
    public class Product : Controller
    {
        public IActionResult Index(int id)
        {
            ViewData["id"] = id;
            ViewBag.Id = id;
            return View(id);
        }
    }
}
