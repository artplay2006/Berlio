using Microsoft.AspNetCore.Mvc;

namespace BerlioWeb.Controllers
{
    public class Service : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }

        [Route("Service/payment-for-travel-on-toll-roads")]
        public IActionResult Service1()
        {
            return View("BelToll");
        }
        [Route("Service/payment-for-fuel-in-IBAN-format")]
        public IActionResult Service2()
        {
            return View("IBAN");
        }
    }
}
