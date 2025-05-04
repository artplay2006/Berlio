using BerlioWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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
        public async Task<IActionResult> Personal(User user)
        {
            var userLogin = User.FindFirst("userLogin")?.Value;
            if (string.IsNullOrEmpty(userLogin)) { return NotFound(); }

            if (userLogin != user.Login)
            {
                // Очищаем ModelState перед передачей данных в представление
                ModelState.Clear();
                // Извлекаем пользователя из БД
                await using (var _context = new BerlioDatabaseContext())
                {
                    var usr = await _context.Users.FindAsync(userLogin);
                    if (usr == null) return NotFound();
                    return View(usr);
                }
            }

            return View(user);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserSettings(User newuserset)
        {
            if (ModelState.IsValid)
            {
                var LoginKey = newuserset.Login;
                await using (var db = new BerlioDatabaseContext()) 
                { 
                    var olduserset = await db.Users.FirstOrDefaultAsync(u=>u.Login== LoginKey);
                    if (olduserset.AreUsersEqualValue(newuserset))
                    {
                        //Console.WriteLine("изменений не произошло");
                        TempData["UpdateMessage"] = "Изменений не произошло";
                        //RedirectToAction("Personal");
                    }
                    else
                    {
                        Console.WriteLine("newuserset и olduserset разные");

                        // Отсоединяем старый объект
                        db.Entry(olduserset).State = EntityState.Detached;

                        // вытаскиваем хеш пароля из jwt токена и сравниваем с измененным паролем
                        //var userHashPassword = User.FindFirst("userHashPassword")?.Value;
                        //if (string.IsNullOrEmpty(userHashPassword)) { return NotFound(); }

                        // если пароль изменен
                        if (newuserset.Password != olduserset.Password)
                        {
                            newuserset.Password = BCrypt.Net.BCrypt.HashPassword(newuserset.Password);
                        }

                        // Присоединяем новый объект
                        db.Users.Update(newuserset);
                        await db.SaveChangesAsync();
                        TempData["UpdateMessage"] = "Изменения сохранены";
                    }
                    //Console.WriteLine("ModelState.IsValid");
                    // Логика обновления данных пользователя
                    // Например, сохранение в базу данных
                }
                // После успешного обновления перенаправляем пользователя
                return RedirectToAction("Personal");
            }
            //Console.WriteLine("ModelState.IsNotValid");
            // Если данные невалидны, возвращаем форму с ошибками
            return RedirectToAction("Personal", newuserset);
        }
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(User user, string? ConfirmPassword)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ConfirmPassword))
                {
                    return Json(new { success = false, errorField = "PasswordMismatchError", errorMessage = "Повтор пароля не написан" });
                }
                // Проверка совпадения паролей
                else if (user.Password != ConfirmPassword)
                {
                    return Json(new { success = false, errorField = "PasswordMismatchError", errorMessage = "Пароли не совпадают." });
                }

                // Проверка reCAPTCHA
                //var response = Request.Form["g-recaptcha-response"];
                //var secretKey = "6Le4M-YqAAAAAEmMhNDumjgRLqqbAqWGdk29i3Z6"; // Замените на ваш секретный ключ
                //var gRecaptchaRequest = new System.Net.Http.HttpClient().GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={response}").Result;
                //var gRecaptchaResponseJson = gRecaptchaRequest.Content.ReadAsStringAsync().Result;
                //var gRecaptchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(gRecaptchaResponseJson);
                //var success = gRecaptchaResponse["success"];
                string googleRecaptchaToken = Request.Form["g-recaptcha-response"].ToString();
                bool isValid = await RecaptchaService.verifyReCaptchaV3(googleRecaptchaToken, "6LfqoPIqAAAAAKmjBGJ22rO4lGj9JqNDCW1P8ZMt", "https://www.google.com/recaptcha/api/siteverify");
                if (!isValid/*success?.ToString()?.ToLower() != "true"*/)
                {
                    return Json(new { success = false, errorField = "ReCaptchaError", errorMessage = "reCAPTCHA не пройдена." });
                }

                // Проверка на существующий логин
                await using (var db = new BerlioDatabaseContext())
                {
                    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
                    if (existingUser != null)
                    {
                        return Json(new { success = false, errorField = "LoginError", errorMessage = "Пользователь с таким логином уже существует." });
                    }

                    // Добавление пользователя в базу данных
                    // Добавление пользователя с зашифрованным паролем
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    //BCrypt.Net.BCrypt.Verify("password", user.Password);
                    db.Add(user);
                    await db.SaveChangesAsync();
                }


                if (HttpContext.Request.Cookies.TryGetValue("jwtToken", out string token))
                {
                    Response.Cookies.Delete("jwtToken", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                }

                // Возвращаем успешный результат
                return Json(new { success = true, redirectUrl = Url.Action("Authorization", "Account") });
            }

            // Если данные невалидны, возвращаем ошибки
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return Json(new { success = false, errors });
        }
        public IActionResult Authorization()
        {
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Authorization(string? Login, string? Password)
        {
            if (string.IsNullOrEmpty(Login))
            {
                return Json(new { success = false, errorField = "LoginError", errorMessage = "Логин не написан" });
            }
            if (string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, errorField = "PasswordError", errorMessage = "Пароль не написан" });
            }

            await using (var _context = new BerlioDatabaseContext())
            {
                if (ModelState.IsValid)
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == Login);
                    if (existingUser == null)
                    {
                        return Json(new { success = false, errorField = "LoginError", errorMessage = "Такого логина не существует" });
                    }
                    // $2a$11$.UtamXhuRnrM1mWGHTXFF.F93NsO.CSKVVMhaTIqL8kAuMcP2NDyS
                    // $2a$11$loTB0E8V2PU.vSFhstnIieBIwtstGiVGg0PzIfWmcw.PdVg9bhYza
                    // $2a$11$pmRkg/Nj9H/HPYN7XD/8suAWehRo1aCYL3LL7LuqYMd/0Zqf5s9pu
                    if (/*existingUser.Password != Password*/!(BCrypt.Net.BCrypt.Verify(Password, existingUser.Password)))
                    {
                        return Json(new { success = false, errorField = "PasswordError", errorMessage = "Неправильный пароль" });
                    }

                    // Проверка reCAPTCHA
                    string googleRecaptchaToken = Request.Form["g-recaptcha-response"].ToString();

                    // Если токена нет вообще (первая попытка)
                    if (string.IsNullOrEmpty(googleRecaptchaToken))
                    {
                        return Json(new { success = false, errorField = "ReCaptchaError", errorMessage = "Пожалуйста, подтвердите, что вы не робот" });
                    }

                    // Проверяем reCAPTCHA v3
                    bool isValid = await RecaptchaService.verifyReCaptchaV3(googleRecaptchaToken, "6LfqoPIqAAAAAKmjBGJ22rO4lGj9JqNDCW1P8ZMt", "https://www.google.com/recaptcha/api/siteverify");

                    // Если v3 не прошла, возвращаем ошибку (на клиенте покажется v2)
                    if (!isValid)
                    {
                        return Json(new { success = false, errorField = "ReCaptchaError", errorMessage = "Пожалуйста, подтвердите, что вы не робот" });
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

                    // Возвращаем успешный результат
                    return Json(new { success = true, redirectUrl = Url.Action("Personal") });
                }
            }
            return Json(new { success = false, errorField = "GeneralError", errorMessage = "Ошибка при обработке запроса" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Удаляем куку с JWT-токеном
            Response.Cookies.Delete("jwtToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Если используете HTTPS
                SameSite = SameSiteMode.Strict
            });

            // Дополнительно можно очистить другие куки

            // Перенаправляем на главную или страницу входа
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckToken()
        {
            if (Request.Cookies.ContainsKey("jwtToken"))
            {
                return RedirectToAction("Personal");
            }
            return RedirectToAction("Authorization");
        }
        // На сервере (Controller)
        [HttpGet("/api/auth/check")]
        public IActionResult CheckAuth()
        {
            var hasToken = Request.Cookies.ContainsKey("jwtToken");
            return Json(new { isAuthenticated = hasToken });
        }
    }
}
