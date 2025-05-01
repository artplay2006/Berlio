using BerlioWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace BerlioWeb.Controllers
{
    public class Product : Controller
    {
        [HttpPost]
        [Route("Product/{linkShortCut}")]  // Добавляем атрибут маршрутизации
        public IActionResult Index(string linkShortCut, string itemJson)
        {
            // Десериализуем JSON в модель
            var item = Newtonsoft.Json.JsonConvert.DeserializeObject(
        itemJson,
        new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
            //Console.WriteLine(item.GetType());
            // Теперь можно использовать item
            // Пример: передать его в представление
            return View(item);
        }

        [HttpGet]
        [Route("Product/{name}")]  // Изменяем маршрут
        public async Task<IActionResult> ShowEquipment(string name)
        {
            await using (var db = new BerlioDatabaseContext())
            {
                var equipment = await db.Equipment.FirstOrDefaultAsync(e=> e.Name == name);
                return View("Index", equipment);
            }
            
        }
    }
}
