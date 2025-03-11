using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BerlioWeb.Models;

public partial class User
{
    [Required(ErrorMessage = "Логин обязателен")]
    [StringLength(25, ErrorMessage = "Логин не должен превышать 25 символов")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(25, ErrorMessage = "Пароль не должен превышать 25 символов")]
    public string? Pasword { get; set; }

    [Required(ErrorMessage = "Электронная почта обязательна")]
    [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Номер телефона обязателен")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "Номер договора обязателен")]
    public int? ContractNumber { get; set; }

    [Required(ErrorMessage = "Место заключения договора обязательно")]
    public string? PlaceOfTheContract { get; set; }
}
