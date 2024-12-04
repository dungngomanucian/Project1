using Project1.Models;

namespace Project1.ViewModels
{
    public class ProductViewModel
    {
        public long ProductId { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public List<TProductDetail>? ProductDetails { get; set; }
    }
}
