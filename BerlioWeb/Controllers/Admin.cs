using BerlioWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BerlioWeb.Controllers
{
    public class Admin : Controller
    {
        private readonly IWebHostEnvironment _env;

        public Admin(IWebHostEnvironment env)
        {
            _env = env;
        }
        [Route("admin/")]
        public IActionResult AuthorizationAdmin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AuthorizationAdmin(string? Login, string? Password)
        {
            // Проверяем заголовок запроса
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!isAjax)
            {
                // Если запрос не AJAX, возвращаем представление
                if (string.IsNullOrEmpty(Login))
                    ModelState.AddModelError("Login", "Логин не написан");

                if (string.IsNullOrEmpty(Password))
                    ModelState.AddModelError("Password", "Пароль не написан");

                return View("Index");
            }

            try
            {
                // Валидация полей
                if (string.IsNullOrEmpty(Login))
                    return Json(new { success = false, errorField = "LoginError", errorMessage = "Логин не написан" });

                if (string.IsNullOrEmpty(Password))
                    return Json(new { success = false, errorField = "PasswordError", errorMessage = "Пароль не написан" });

                await using (var _context = new BerlioDatabaseContext())
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == Login);

                    if (existingUser == null)
                        return Json(new { success = false, errorField = "LoginError", errorMessage = "Такого логина не существует" });

                    if (existingUser.Role != "admin")
                        return Json(new { success = false, errorField = "GeneralError", errorMessage = "Вы не админ" });

                    if (!BCrypt.Net.BCrypt.Verify(Password, existingUser.Password))
                        return Json(new { success = false, errorField = "PasswordError", errorMessage = "Неправильный пароль" });

                    // Генерация JWT-токена
                    JwtConfig _jwtConfig = new JwtConfig
                    {
                        Key = "1234567890abcdefghijklmnopqrstuvwxyz",
                        Issuer = "MyAuthServer",
                        Audience = "berlio"
                    };

                    var token = JwtUtil.GenerateJwtToken(existingUser, _jwtConfig);

                    // Установите куку или передайте токен клиенту
                    Response.Cookies.Append("jwtToken", token, new CookieOptions
                    {
                        HttpOnly = true,  // Защита от XSS
                        Secure = true,    // Только через HTTPS
                        Expires = DateTime.Now.AddHours(1)
                    });

                    return Json(new { success = true, redirectUrl = Url.Action("AdminPanel") });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errorField = "GeneralError",
                    errorMessage = $"Ошибка сервера: {ex.Message}"
                });
            }
        }
        public async Task<IActionResult> AdminPanel()
        {
            var userLogin = User.FindFirst("userLogin")?.Value;
            await using (var _context = new BerlioDatabaseContext())
            {
                var usr = await _context.Users.FindAsync(userLogin);
                if (usr == null)
                {
                    return NotFound("Пользователь не найден"); // Или другая обработка
                }
                ViewData["adminAcc"] = usr;

                ViewBag.Programs = await _context.Programs.ToListAsync();

                ViewBag.Equipments = await _context.Equipment.ToListAsync();

                ViewBag.Balances = await _context.BalancesOfServices.ToListAsync();

                ViewBag.DepositHistory = await _context.DepositHistories.ToListAsync();

                var os = await _context.OrderSells
    .Select(os => new OrderDetail
    {
        Id = os.Id,
        Count = os.Count,
        Client = os.Client,
        Finished = os.Finished,
        Type = os.Type,
        Name = os.Type == "Program"
            ? _context.Programs
                .Where(p => p.Id == os.Idproduct)
                .Select(p => p.Name)
                .FirstOrDefault()
            : os.Type == "Equipment"
                ? _context.Equipment
                    .Where(e => e.Id == os.Idproduct)
                    .Select(e => e.Name)
                    .FirstOrDefault()
                : null,
        Image = os.Type == "Program"
            ? _context.Programs
                .Where(p => p.Id == os.Idproduct)
                .Select(p => p.Image)
                .FirstOrDefault()
            : os.Type == "Equipment"
                ? _context.Equipment
                    .Where(e => e.Id == os.Idproduct)
                    .Select(e => e.Image)
                    .FirstOrDefault()
                : null
    })
    .Where(x => x.Name != null)
    .ToListAsync();
                ViewBag.OrderSell = os;

                var ed = await _context.EquipmentDeliveries.Select(ed => new DeliveryDetail
                {
                    Id = ed.Id,
                    Addressdelivery = ed.Addressdelivery,
                    Timedelivery = ed.Timedelivery,
                    Name = _context.Equipment.Where(e=>e.Id==
                    _context.OrderSells.Where(o=>o.Id==ed.Idordersell).Select(o=>o.Idproduct).FirstOrDefault())
                    .Select(o=>o.Name)
                    .FirstOrDefault(),
                    Image = _context.Equipment.Where(e => e.Id ==
                    _context.OrderSells.Where(o => o.Id == ed.Idordersell).Select(o => o.Idproduct).FirstOrDefault())
                    .Select(o => o.Image)
                    .FirstOrDefault()
                }).ToListAsync();

                ViewBag.EquipmentDelivery = ed;

                ViewBag.Users = await _context.Users.ToListAsync();

                return View();
            }

        }
        [HttpGet]
        public IActionResult GetProgram(int id)
        {
            using (var _context = new BerlioDatabaseContext())
            {
                var program = _context.Programs
                    .FirstOrDefault(p => p.Id == id);

                return Json(new
                {
                    id = program.Id,
                    name = program.Name,
                    shortDescription = program.ShortDescription,
                    longDescription = program.LongDescription,
                    image = program.Image,
                    pathtodownload = program.Pathtodownload
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveProgram([FromForm] BerlioWeb.Models.Program model, [FromForm] IFormFile? NewImage, [FromForm] IFormFile? NewPathtodownload)
        {
            // Валидация модели
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            Console.WriteLine(model?.Image);
            // Обработка файла
            if (NewImage != null && NewImage.Length > 0)
            {
                var fileName = NewImage.FileName;
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                // Если файл с таким именем уже существует и это не текущее изображение программы
                //if (System.IO.File.Exists(filePath) && fileName == model.Image && forceOverwrite != "true")
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        needConfirmation = true,
                //        message = $"Файл с именем '{fileName}' уже существует. Хотите заменить его?",
                //        fileName = fileName
                //    });
                //}
                if (System.IO.File.Exists(filePath) && fileName == model.Image)
                {
                    System.IO.File.Delete(filePath);
                }
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await NewImage.CopyToAsync(stream);
                }

                model.Image = fileName;
            }
            if (NewPathtodownload != null && NewPathtodownload.Length > 0)
            {
                var exePath = Path.Combine(_env.WebRootPath, "programs", NewPathtodownload.FileName);
                if (System.IO.File.Exists(exePath) && NewPathtodownload.FileName == model.Pathtodownload)
                {
                    System.IO.File.Delete(exePath);
                }
                await using (var stream = new FileStream(exePath, FileMode.Create))
                {
                    await NewPathtodownload.CopyToAsync(stream);
                }
                model.Pathtodownload = NewPathtodownload.FileName;
            }

            await using (var _context = new BerlioDatabaseContext())
            {
                var existingProgram = await _context.Programs.FindAsync(model.Id);
                _context?.Entry(existingProgram).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> AddProgram(
    [FromForm] string Name,
    [FromForm] string? ShortDescription,
    [FromForm] string? LongDescription,
    [FromForm] string? Image,
    [FromForm] IFormFile? NewImage,
    [FromForm] IFormFile? NewPathtodownload)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return BadRequest(new { error = "Название программы обязательно" });
                }
                Console.WriteLine(NewPathtodownload?.FileName);
                if (NewPathtodownload?.FileName == null || NewPathtodownload.FileName.Length == 0)
                {
                    return BadRequest(new { error = "Файл программы обязателен" });
                }

                var newProgram = new BerlioWeb.Models.Program
                {
                    Name = Name.Trim(),
                    ShortDescription = ShortDescription?.Trim(),
                    LongDescription = LongDescription?.Trim(),
                };

                // Обработка изображения
                if (NewImage != null && NewImage.Length > 0)
                {
                    var imageFileName = NewImage.FileName;
                    var imagePath = Path.Combine(_env.WebRootPath, "images", imageFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                    await using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await NewImage.CopyToAsync(stream);
                    }

                    newProgram.Image = imageFileName;
                }
                else
                {
                    newProgram.Image = Image;
                }

                    // Обработка EXE-файла
                    var exeFileName = NewPathtodownload.FileName;
                var exePath = Path.Combine(_env.WebRootPath, "programs", exeFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(exePath));

                await using (var stream = new FileStream(exePath, FileMode.Create))
                {
                    await NewPathtodownload.CopyToAsync(stream);
                }

                newProgram.Pathtodownload = exeFileName;

                // Сохранение в БД
                await using (var context = new BerlioDatabaseContext())
                {
                    await context.Programs.AddAsync(newProgram);
                    await context.SaveChangesAsync();
                }

                return Ok(new { success = true, id = newProgram.Id });
            }
            catch (Exception ex)
            {
                // Логирование полной ошибки
                Console.WriteLine($"Ошибка при добавлении программы: {ex}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    detailedError = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProgram(int? id)
        {
            if (id == null)
            {
                return BadRequest(new { error = "Нет id программы для удаления" });
            }
            await using (var _context = new BerlioDatabaseContext())
            {
                var rp = await _context.Programs.FirstOrDefaultAsync(p => p.Id == id);
                if (rp != null)
                {
                    _context.Programs.Remove(rp);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest(new { error = $"Нет программы с id: {id}" });
            }
        }
        // КОНТРОЛЛЕРЫ ДЛЯ ОБОРУДОВАНИЯ
        [HttpPost]
        public async Task<IActionResult> DeleteEquipment(int? id)
        {
            if (id == null)
            {
                return BadRequest(new { error = "Нет id программы для оборудования" });
            }
            await using (var _context = new BerlioDatabaseContext())
            {
                var rp = await _context.Equipment.FirstOrDefaultAsync(p => p.Id == id);
                if (rp != null)
                {
                    _context.Equipment.Remove(rp);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                return BadRequest(new { error = $"Нет оборудования с id: {id}" });
            }
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AddEquipment(
    [FromForm] string Name,
    [FromForm] string? ShortDescription,
    [FromForm] string? LongDescription,
    [FromForm] int Countavailability, // Изменили на не-nullable
    [FromForm] IFormFile? NewImage)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(Name))
                    return BadRequest(new { error = "Название оборудования обязательно" });

                if (Countavailability < 0)
                    return BadRequest(new { error = "Количество не может быть отрицательным" });

                var newEquipment = new Equipment
                {
                    Name = Name.Trim(),
                    ShortDescription = ShortDescription?.Trim(),
                    LongDescription = LongDescription?.Trim(),
                    Countavailability = Countavailability,
                };

                // Обработка изображения
                if (NewImage != null && NewImage.Length > 0)
                {
                    var imageFileName = NewImage.FileName;
                    var imagePath = Path.Combine(_env.WebRootPath, "images", imageFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await NewImage.CopyToAsync(stream);
                    }

                    newEquipment.Image = imageFileName;
                }
                else
                {
                    newEquipment.Image = "default.jpg";
                }

                // Сохранение в БД
                await using (var context = new BerlioDatabaseContext())
                {
                    await context.Equipment.AddAsync(newEquipment);
                    await context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    id = newEquipment.Id,
                    image = newEquipment.Image
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    detailedError = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveEquipment(
    [FromForm] Equipment model,
    [FromForm] IFormFile? NewImage)
        {
            try
            {
                // Валидация модели
                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest(new { error = "Название оборудования обязательно" });

                if (model.Countavailability < 0)
                    return BadRequest(new { error = "Количество не может быть отрицательным" });

                // Обработка изображения
                if (NewImage != null && NewImage.Length > 0)
                {
                    var fileName = NewImage.FileName;
                    var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                    // Создаем директорию, если её нет
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await NewImage.CopyToAsync(stream);
                    }

                    // Удаляем старое изображение, если оно есть
                    if (!string.IsNullOrEmpty(model.Image))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, "images", model.Image);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    model.Image = fileName;
                }

                using (var context = new BerlioDatabaseContext())
                {
                    if (model.Id == 0)
                    {
                        context.Equipment.Add(model);
                    }
                    else
                    {
                        var existing = await context.Equipment.FindAsync(model.Id);
                        if (existing == null) return NotFound();
                        context.Entry(existing).CurrentValues.SetValues(model);
                    }

                    await context.SaveChangesAsync();
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, new
                {
                    error = "Внутренняя ошибка сервера",
                    detailedError = ex.Message
                });
            }
        }
        [HttpGet]
        public IActionResult GetEquipment(int id)
        {
            using (var _context = new BerlioDatabaseContext())
            {
                var equipment = _context.Equipment
                    .FirstOrDefault(p => p.Id == id);

                return Json(new
                {
                    id = equipment.Id,
                    name = equipment.Name,
                    shortDescription = equipment.ShortDescription,
                    longDescription = equipment.LongDescription,
                    image = equipment.Image,
                    countavailability = equipment.Countavailability
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Неверные данные" });
            }

            await using (var _context = new BerlioDatabaseContext())
            {
                var programId = model.ProgramId;
                var newBalance = model.NewBalance;

                if (programId == null || newBalance == null)
                {
                    return BadRequest(new { success = false, message = "Недостаточно данных" });
                }

                var balance = await _context.BalancesOfServices.FirstOrDefaultAsync(b => b.Id == programId);
                if (balance == null)
                {
                    return NotFound(new { success = false, message = "Баланс не найден" });
                }

                balance.Balance = newBalance;
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
        }
        public class UpdateBalanceDto
        {
            [Required]
            public int ProgramId { get; set; }
            [Range(0.01, 1000000)]
            public double NewBalance { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateFinished([FromBody] UpdateOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Неверные данные" });
            }
            await using (var _context = new BerlioDatabaseContext())
            {
                var q = await _context.OrderSells.FirstOrDefaultAsync(os => os.Id == model.ProgramId);
                if (q != null)
                {
                    q.Finished = model.NewFinished;
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true });
                }
                return BadRequest(new { success = false, message = $"Нет заказа с id: {model.ProgramId}" });
            }
        }
        public class UpdateOrderDto
        {
            [Required]
            public int ProgramId { get; set; }
            
            public bool NewFinished { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateUserRoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Неверные данные" });
            }
            await using (var _context = new BerlioDatabaseContext())
            {
                var q = await _context.Users.FirstOrDefaultAsync(os => os.Login == model.Login);
                if (q != null)
                {
                    q.Role = model.NewRole;
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true });
                }
                return BadRequest(new { success = false, message = $"Нет пользоветеля с Login: {model.Login}" });
            }
        }
        public class UpdateUserRoleDto
        {
            [Required]
            public string? Login { get; set; }

            public string? NewRole { get; set; }
        }
    }
}
