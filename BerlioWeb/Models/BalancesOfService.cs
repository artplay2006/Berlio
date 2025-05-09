﻿using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class BalancesOfService
{
    public int Id { get; set; }

    public string Nameservice { get; set; } = null!;

    public double Balance { get; set; }

    public string Loginclient { get; set; } = null!;

    public virtual User LoginclientNavigation { get; set; } = null!;
}
