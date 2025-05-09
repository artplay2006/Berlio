using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class DepositHistory
{
    public int Id { get; set; }

    public DateTime Timedeposit { get; set; }

    public double Sumofmoney { get; set; }

    public string TypeBalance { get; set; } = null!;

    public string Loginclient { get; set; } = null!;

    public virtual User LoginclientNavigation { get; set; } = null!;
}
