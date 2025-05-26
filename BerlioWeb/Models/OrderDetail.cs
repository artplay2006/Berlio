namespace BerlioWeb.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string? Client { get; set; }
        public bool Finished { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
    }
}
