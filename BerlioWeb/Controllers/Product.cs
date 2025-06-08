using BerlioWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BerlioWeb.Controllers
{
    public class Product : Controller
    {
        private readonly IWebHostEnvironment _env;

        public Product(IWebHostEnvironment env)
        {
            _env = env;
        }
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
                                                o.Client == orderData.Client &&
                                                o.Type == orderData.Type);

                    if (existingOrder != null)
                    {
                        return BadRequest(new { message = "Вы уже оформляли такой заказ" });
                    }

                    await db.OrderSells.AddAsync(orderData);

                    await db.SaveChangesAsync();
                    
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
                var userLogin = User.FindFirst("userLogin")?.Value;
                await using (var db = new BerlioDatabaseContext())
                {
                    // Получаем последний заказ с явным указанием порядка сортировки
                    var lastOrder = await db.OrderSells
                        .OrderByDescending(o => o.Id) // Сортируем по ID в обратном порядке
                        .FirstOrDefaultAsync(); // Берем первый элемент после сортировки

                    if (lastOrder == null)
                    {
                        return BadRequest(new { message = "Не найдено ни одного заказа" });
                    }

                    equipmentDelivery.Idordersell = lastOrder.Id;

                    if (db.Equipment.FirstOrDefault(e => e.Id == lastOrder.Idproduct)?.Name == "Электронные карточки")
                    {
                        BalancesOfService balancesOfService = new BalancesOfService
                        {
                            Nameservice = "Электронные карточки",
                            Balance = 0,
                            Loginclient = userLogin
                        };
                        await db.BalancesOfServices.AddAsync(balancesOfService);
                    }

                    await db.EquipmentDeliveries.AddAsync(equipmentDelivery);
                    await db.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        deliveryId = equipmentDelivery.Id,
                        orderId = equipmentDelivery.Idordersell
                    });
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.Error.WriteLine($"Ошибка при создании доставки: {ex}");
                return BadRequest(new
                {
                    message = "Произошла ошибка при обработке запроса",
                    detailed = ex.Message
                });
            }
        }
        [HttpGet]
        [Route("Product/CheckExisting")]
        public async Task<IActionResult> CheckExistingDevice()
        {
            try
            {
                await using (var _context = new BerlioDatabaseContext())
                {
                    var userLogin = User.FindFirst("userLogin")?.Value;

                    if (string.IsNullOrEmpty(userLogin))
                    {
                        return BadRequest(new { exists = false });
                    }

                    var exists = await _context.BalancesOfServices
                        .AnyAsync(bos => bos.Loginclient == userLogin && bos.Nameservice == "BelToll");

                    return Ok(new { exists });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exists = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("Product/Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDevice()
        {
            try
            {
                await using (var _context = new BerlioDatabaseContext())
                {
                    var userLogin = User.FindFirst("userLogin")?.Value;

                    if (string.IsNullOrEmpty(userLogin))
                    {
                        return BadRequest(new { success = false, message = "Пользователь не авторизован" });
                    }

                    // Проверяем существование устройства
                    var existingDevice = await _context.BalancesOfServices
                        .FirstOrDefaultAsync(bos => bos.Loginclient == userLogin && bos.Nameservice == "BelToll");

                    if (existingDevice != null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Устройство BelToll уже зарегистрировано для этого пользователя"
                        });
                    }

                    // Создаем новое устройство
                    var newDevice = new BalancesOfService
                    {
                        Nameservice = "BelToll", // Исправлено с "Электронные карточки" на "BelToll"
                        Balance = 0,
                        Loginclient = userLogin
                    };

                    await _context.BalancesOfServices.AddAsync(newDevice);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Бортовое устройство BelToll успешно зарегистрировано"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Внутренняя ошибка сервера: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> Download(int id)
        {
            await using (var db = new BerlioDatabaseContext())
            {
                var program = await db.Programs.FirstOrDefaultAsync(p=>p.Id==id);
                if (program == null)
                {
                    return NotFound($"Нет программы с таким id: {id}");
                }
            
                // нужно провести путь до папки wwwroot и там найти файл regional-centre.exe
                // Получаем путь к папке wwwroot
                var webRootPath = _env.WebRootPath;

            // Формируем полный путь к файлу
            var filePath = Path.Combine(webRootPath, "programs", program.Pathtodownload);

            // Проверяем существование файла
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Файл не найден");
            }

            // Определяем MIME-тип для ZIP-архива
            var mimeType = "application/zip";

            // Возвращаем файл для скачивания
            return PhysicalFile(filePath, mimeType, program.Pathtodownload);
            }
        }
    }
}
