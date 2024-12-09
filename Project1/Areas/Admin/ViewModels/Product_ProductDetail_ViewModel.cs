using Project1.Models;
using System.Drawing;

namespace Project1.Areas.Admin.ViewModels
{
    public class Product_ProductDetail_ViewModel
    {
        public TProduct Product { get; set; }
        public TProductDetail ProductDetail { get; set; }

        public IFormFile Image { get; set; }
    }
}
