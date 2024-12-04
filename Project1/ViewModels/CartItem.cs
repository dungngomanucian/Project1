namespace Project1.ViewModels
{
    public class CartItem
    {
        public long ProductDetailId { get; set; }

        public string? Image { get; set; }

        public string? Name { get; set; }

        public double? Price { get; set; }

        public long? SizeId { get; set; }

        public long? CrustId { get; set; }

        public long? Quantity { get; set; }

        public double? ThanhTien => Quantity * Price;

        public bool isPizza { get; set; }
    }
}
