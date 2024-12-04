namespace Project1.ViewModels
{
    public class CategoryViewModel
    {
        public long CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public List<ProductViewModel> Products { get; set; }
    }
}
