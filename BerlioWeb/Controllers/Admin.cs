using BerlioWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BerlioWeb.Controllers
{
    public class Admin : Controller
    {
        [Route("admin/")]
        public IActionResult AuthorizationAdmin()
        {
            //TempData["AdminRedirectData"] = true;
            //return RedirectToAction("Authorization","Account");
            return View();
        }
        //[Route("admin/")]
        //public IActionResult AuthAdmin()
        //{
        //    return View("~/Views/Account/Authorization.cshtml");
        //}
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

                    return Json(new { success = true, redirectUrl = Url.Action("Personal") });
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
    }
}
