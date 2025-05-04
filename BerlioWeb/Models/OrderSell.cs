using System;
using System.Collections.Generic;

namespace BerlioWeb.Models;

public partial class OrderSell
{
    public int Id { get; set; }

    public int Idproduct { get; set; }

    public int Count { get; set; }

    public string Type { get; set; } = null!;

    public string Client { get; set; } = null!;

    public bool Finished { get; set; }

    public virtual ICollection<EquipmentDelivery> EquipmentDeliveries { get; set; } = new List<EquipmentDelivery>();
}
