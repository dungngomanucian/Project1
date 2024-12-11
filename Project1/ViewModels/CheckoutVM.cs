using System.ComponentModel.DataAnnotations;

namespace Project1.ViewModels
{
	public class CheckoutVM
	{

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string? HoTen { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string? DiaChi { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có đủ 10 chữ số")]
        public string? DienThoai { get; set; }
		public string? GhiChu { get; set; }
	}
}
