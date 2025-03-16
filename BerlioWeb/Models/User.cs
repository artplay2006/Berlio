using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class User
{
    public string Login { get; set; } = null!;

    public string? Pasword { get; set; }

    public string? Email { get; set; }

    public string? Telephone { get; set; }

    public int? ContractNumber { get; set; }

    public string? PlaceOfTheContract { get; set; }
}
