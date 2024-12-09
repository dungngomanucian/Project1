using System;
using System.Collections.Generic;
using Project1.Models;

namespace Project1.Areas.Admin.ViewModels
{
    public class ProductDetail_ViewModel
    {
        public string CategoryName { get; set; }
        public long ProductDetailId { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public string Crust {  get; set; }
        public double? Price { get; set; }
        public long? Quantity { get; set; }
        public string ProductImage { get; set; } // Thêm thuộc tính ảnh sản phẩm
    }

    public class OrderDetail_ViewModel
    {
        public List<ProductDetail_ViewModel> SelectedProducts { get; set; }
        public TOrder Order { get; set; }
        public TOrderCombo OCombo { get; set; }
        public double? TotalPriceAfterUseVoucher { get; set; }
    }
}
