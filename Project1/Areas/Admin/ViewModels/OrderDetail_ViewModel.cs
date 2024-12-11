using System;
using System.Collections.Generic;
using Project1.Models;

namespace Project1.Areas.Admin.ViewModels
{
    
    public class OrderDetail_ViewModel
    {
        public List<ProductDetail_ViewModel> SelectedProducts { get; set; }
        public TOrder Order { get; set; }
        public TOrderCombo OCombo { get; set; }
        public double? TotalPriceAfterUseVoucher { get; set; }
    }
}
