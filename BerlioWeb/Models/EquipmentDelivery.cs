using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class EquipmentDelivery
{
    public int Id { get; set; }

    public int Idordersell { get; set; }

    public string Addressdelivery { get; set; } = null!;

    public DateTime Timedelivery { get; set; }

    public virtual OrderSell IdordersellNavigation { get; set; } = null!;
}
