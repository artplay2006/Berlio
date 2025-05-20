using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class Program
{
    public string Name { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? LongDescription { get; set; }

    public string? Image { get; set; }

    public int Id { get; set; }

    public string? Pathtodownload { get; set; }
}
