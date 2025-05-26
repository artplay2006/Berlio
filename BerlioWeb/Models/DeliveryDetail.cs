namespace BerlioWeb.Models
{
    public class DeliveryDetail
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Image { get; set; }

        public string Addressdelivery { get; set; } = null!;

        public DateTime Timedelivery { get; set; }
    }
}
