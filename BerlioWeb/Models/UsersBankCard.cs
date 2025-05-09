using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class UsersBankCard
{
    public int Id { get; set; }

    public string Numcard { get; set; } = null!;

    public string ValidityPeriod { get; set; } = null!;

    public int Cvc { get; set; }

    public string Loginclient { get; set; } = null!;

    public virtual User LoginclientNavigation { get; set; } = null!;
}
