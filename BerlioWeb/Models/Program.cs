using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BerlioWeb.Models;

public partial class Program
{
    [Required(ErrorMessage = "Название программы обязательно")]
    [StringLength(50, ErrorMessage = "Максимальная длина названия - 50 символов")]
    public string Name { get; set; } = null!;
    [StringLength(646, ErrorMessage = "Максимальная длина названия - 646 символов")]
    public string? ShortDescription { get; set; }

    public string? LongDescription { get; set; }
    [Required(ErrorMessage = "Картинка программы обязательна")]
    public string? Image { get; set; }

    public int Id { get; set; }
    [Required(ErrorMessage = "Файл программы обязателен")]
    public string? Pathtodownload { get; set; }
}
