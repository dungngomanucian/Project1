using System.ComponentModel.DataAnnotations;

namespace Project1.ViewModels
{
    public class UserProfileViewModel
    {
        public long UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập biệt danh")]
        public string Nickname { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải là 10 chữ số")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }
    }
}
