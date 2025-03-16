using BerlioWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;

namespace BerlioWeb.Controllers
{
    public class Account : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Personal()
        {
            // Получаем UserId из claims
            var userLogin = User.FindFirst("userLogin")?.Value;
            if (userLogin == null) return RedirectToAction("Authorization");

            // Извлекаем пользователя из БД
            await using (var _context = new BerlioDatabaseContext())
            {
                User? user = await _context.Users.FindAsync(userLogin);
                if (user == null) return NotFound();

                return View(user);
            }
        }
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(User user, string? ConfirmPassword)
        {
            Console.WriteLine("Task<IActionResult> Registration");
            //ModelState.IsValid — это свойство, которое используется для проверки валидности данных модели в контроллере.
            //    Оно возвращает true, если все атрибуты модели прошли проверку на соответствие заданным аннотациям валидации,
            //    и false в противном случае.
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ConfirmPassword))
                {
                    ViewData["PasswordMismatchError"] = "Повтор пароля не написан";
                    return View();
                }
                // Проверка совпадения паролей
                else if (user.Pasword != ConfirmPassword)
                {
                    ViewData["PasswordMismatchError"] = "Пароли не совпадают.";
                    return View();
                }

                // Проверка reCAPTCHA
                var response = Request.Form["g-recaptcha-response"];
                var secretKey = "6Le4M-YqAAAAAEmMhNDumjgRLqqbAqWGdk29i3Z6"; // Замените на ваш секретный ключ
                var gRecaptchaRequest = new System.Net.Http.HttpClient().GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={response}").Result;
                var gRecaptchaResponseJson = gRecaptchaRequest.Content.ReadAsStringAsync().Result;
                var gRecaptchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(gRecaptchaResponseJson);
                var success = gRecaptchaResponse["success"];

                if (success?.ToString()?.ToLower() != "true")
                {
                    ViewData["ReCaptchaError"] = "reCAPTCHA не пройдена.";
                    return View();
                }

                // Добавление пользователя в базу данных
                await using (var db = new BerlioDatabaseContext())
                {
                    db.Add(user);
                    await db.SaveChangesAsync();
                }
                return RedirectToAction("Authorization", "Account");
            }
            return View();
        }
        public IActionResult Authorization()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authorization(string? Login, string? Pasword)
        {
            if (string.IsNullOrEmpty(Login)) { ViewData["LoginError"] = "Логин не написан"; return View(); }
            if (string.IsNullOrEmpty(Pasword)) { ViewData["PasswordError"] = "Пароль не написан"; return View(); }

            await using (var _context = new BerlioDatabaseContext())
            {
                if (ModelState.IsValid)
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == Login);
                    if (existingUser == null)
                    {
                        ViewData["LoginError"] = "Такого логина не существует";
                        return View();
                    }

                    if (existingUser.Pasword != Pasword)
                    {
                        ViewData["PasswordError"] = "Неправильный пароль";
                        return View();
                    }

                    string googleRecaptchaToken = Request.Form["g-recaptcha-response"].ToString();
                    //Console.WriteLine(googleRecaptchaToken);
                    bool isValid = await RecaptchaService.verifyReCaptchaV3(googleRecaptchaToken, "6LfqoPIqAAAAAKmjBGJ22rO4lGj9JqNDCW1P8ZMt", "https://www.google.com/recaptcha/api/siteverify");
                    if (!isValid)
                    {
                        // ПЕРЕДЕЛАТЬ ВЫВОД ОШИБОК БЕЗ ПЕРЕЗАГРУЗКИ СТРАНИЦЫ
                    }

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

                    // Возвращаем токен в JSON-формате;
                    return RedirectToAction("Personal");
                }
            }
            return View();
        }
    }
}
