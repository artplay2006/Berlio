using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BerlioWeb.Models;

public partial class User
{
    [Key]
    [Required(ErrorMessage = "Логин обязателен.")]
    [StringLength(25, ErrorMessage = "Логин не должен превышать 25 символов.")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Пароль обязателен.")]
    [StringLength(60, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 25 символов.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Телефон обязателен.")]
    [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Некорректный формат телефона.")]
    public string? Telephone { get; set; }

    public string? Role { get; set; }

    public bool AreUsersEqualValue(User user2)
    {
        if (user2 == null)
            return false;

        return Login == user2.Login && Password == user2.Password &&
               Email == user2.Email && Telephone == user2.Telephone;// мб надо добавить role
    }
    //public virtual BalancesOfService? BalancesOfService { get; set; }
    public virtual ICollection<BalancesOfService> BalancesOfServices { get; set; } = new List<BalancesOfService>();

    public virtual ICollection<DepositHistory> DepositHistories { get; set; } = new List<DepositHistory>();

    public virtual ICollection<UsersBankCard> UsersBankCards { get; set; } = new List<UsersBankCard>();
}
