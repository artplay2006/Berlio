using BerlioWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BerlioWeb.Controllers
{
    public class Category : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Programs()
        {
            CategoryModel model = new CategoryModel();

            ViewData["Title"] = "Программы";
            model.pahtMainPage = "../images/programs.png";
            model.titlePage = "Программное обеспечение";
            model.descriptionPage = "НП ООО «Берлио» имеет большой опыт в<br>разработке и сопровождении<br>программного обеспечения.<br>Программное обеспечение нашей<br>компании используется в системе<br>безналичных расчетов на АЗС, СТО,<br>пунктах взимания дорожных сборов, в<br>магазинах и в других торговых объектах<br>РБ и стран ближнего зарубежья.";

            await using (var db = new BerlioDatabaseContext())
            {
                ViewBag.Items = await db.Programs.ToArrayAsync();
            }

            return View("Index",model);
        }
        public async Task<IActionResult> Equipments()
        {
            CategoryModel model = new CategoryModel();

            ViewData["Title"] = "Оборудование";
            model.pahtMainPage = "../images/equipments.png";
            model.titlePage = "Оборудование";
            model.descriptionPage = "Оборудование «Берлио» автоматизирует АЗС, управляя отпуском топлива и безналичными расчетами.<br>Основные компоненты:<br>— ПО для учета и отчетности («Региональный центр», «Расчетный центр»);\n— Система АИИСУ ТПОН для контроля ТРК и резервуаров;\n— Считыватели карт (УСЭК) для самообслуживания.<br>Технические особенности:<br>— Работает при -40…+50°C;<br>— Совместимость с колонками мировых брендов;<br>— Защита данных шифрованием.<br>Эффекты:<br>Сокращение затрат, рост пропускной способности, прозрачность операций.<br>Сертифицировано в РФ и РБ.";

            await using (var db = new BerlioDatabaseContext())
            {
                ViewBag.Items = await db.Equipment.ToArrayAsync();
            }

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

            return View();
        }
    }
}
