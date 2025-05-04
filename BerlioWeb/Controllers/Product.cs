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
        [HttpPost]
        [Route("Product/ProcessOrder")]
        public async Task<IActionResult> ProcessOrder([FromBody] OrderSell orderData)
        {
            try
            {
                await using (var db = new BerlioDatabaseContext())
                {
                    // Заполняем недостающие поля
                    orderData.Client = User.FindFirst("userLogin")?.Value;

                    // Проверяем существующий заказ
                    var existingOrder = await db.OrderSells
                        .FirstOrDefaultAsync(o => o.Idproduct == orderData.Idproduct &&
                                                o.Client == orderData.Client);

                    if (existingOrder != null)
                    {
                        return BadRequest(new { message = "Вы уже оформляли такой заказ" });
                    }

                    await db.OrderSells.AddAsync(orderData);
                    Console.WriteLine($"{orderData.Id}\n{orderData.Idproduct}\n{orderData.Count}\n{orderData.Client}\n{orderData.Finished}");
                    await db.SaveChangesAsync();
                    //Console.WriteLine($"{orderData.Id}\n{orderData.Idproduct}\n{orderData.Count}\n{orderData.Client}\n{orderData.Finished}");
                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("Product/CreateDelivery")]
        public async Task<IActionResult> CreateDelivery([FromBody] EquipmentDelivery equipmentDelivery)
        {
            try
            {
                await using (var db = new BerlioDatabaseContext())
                {
                    // Заполняем недостающие поля
                    var lastOrder = await db.OrderSells.LastOrDefaultAsync();
                    if (lastOrder==null)
                    {
                        return BadRequest(new { message = "Заказ не был оформлен. Пиздец ошибка" });
                    }
                    equipmentDelivery.Idordersell = lastOrder.Id;

                    await db.EquipmentDeliveries.AddAsync(equipmentDelivery);
                    await db.SaveChangesAsync();
                    //Console.WriteLine($"{equipmentDelivery.Id}\n{equipmentDelivery.Idordersell}\n{equipmentDelivery.Addressdelivery}\n{equipmentDelivery.Timedelivery}");
                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
