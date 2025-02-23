using BerlioWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BerlioWeb.Controllers
{
    public class Category : Controller
    {
        public IActionResult Index()
        {
            //CategoryModel model = new CategoryModel();

            //switch (pageName)
            //{
            //    case "Page1":
            //        model.pahtMainPage = "Заголовок страницы 1";
            //        model.titlePage = "<p>Программы</p>";
            //        model.descriptionPage = "~/images/page1.png";
            //        ViewData["Title"] = "Программы";
            //        break;
            //    case "Page2":
            //        model.pahtMainPage = "Заголовок страницы 2";
            //        model.titlePage = "<p>Оборудование</p>";
            //        model.descriptionPage = "~/images/page2.png";
            //        ViewData["Title"] = "Оборудование";
            //        break;
            //    //case "Page3":
            //    //    model.Title = "Заголовок страницы 3";
            //    //    model.Content = "<p>Содержимое страницы 3</p>";
            //    //    model.ImageUrl = "~/images/page3.png";
            //    //    break;
            //    //default:
            //    //    model.Title = "Главная страница";
            //    //    model.Content = "<p>Основное содержимое сайта</p>";
            //    //    model.ImageUrl = "~/images/mainpage.png";
            //    //    break;
            //}

            return View();
        }
        public IActionResult Programs()
        {
            CategoryModel model = new CategoryModel();

            ViewData["Title"] = "Программы";
            model.pahtMainPage = "../images/programs.png";
            model.titlePage = "Программное обеспечение";
            model.descriptionPage = "НП ООО «Берлио» имеет большой опыт в<br>разработке и сопровождении<br>программного обеспечения.<br>Программное обеспечение нашей<br>компании используется в системе<br>безналичных расчетов на АЗС, СТО,<br>пунктах взимания дорожных сборов, в<br>магазинах и в других торговых объектах<br>РБ и стран ближнего зарубежья.";

            return View("Index",model);
        }
        public IActionResult Equipments()
        {
            CategoryModel model = new CategoryModel();

            ViewData["Title"] = "Оборудование";
            model.pahtMainPage = "../images/equipments.png";
            model.titlePage = "Оборудование";
            model.descriptionPage = "Оборудование НП ООО \"Берлио\"";

            return View("Index", model);
        }
        public IActionResult ForClients()
        {
            CategoryModel model = new CategoryModel();

            ViewData["Title"] = "Для клиентов";
            model.pahtMainPage = "../images/forclients.png";
            model.titlePage = "Для клиентов";
            model.descriptionPage = "";

            return View("Index", model);
        }
        public IActionResult Services()
        {
            CategoryModel model = new CategoryModel();

            ViewData["Title"] = "Сервисы";
            model.pahtMainPage = "../images/services.png";
            model.titlePage = "Сервисы";
            model.descriptionPage = "";

            return View("Index", model);
        }
    }
}
