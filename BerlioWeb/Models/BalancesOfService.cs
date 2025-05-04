using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class BalancesOfService
{
    public int Id { get; set; }

    public string Client { get; set; } = null!;

    public string Nameservice { get; set; } = null!;

    public double Balance { get; set; }

    public virtual User ClientNavigation { get; set; } = null!;
}
