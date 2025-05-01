using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class Equipment
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? LongDescription { get; set; }

    public string? Image { get; set; }
}
