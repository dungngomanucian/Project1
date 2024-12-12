using System.ComponentModel.DataAnnotations;

namespace Project1.ViewModels
{
    public class RegisterUserViewModel
    {
        [Key]
        public long UserId { get; set; }
        public long? RoleId { get; set; }
        public string? Code { get; set; }
        [Required(ErrorMessage = "Tên không được để trống")]
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Nickname { get; set; }
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có đủ 10 chữ số")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Mật khẩu xác nhận không được để trống.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận không trùng khớp.")]
        public string? ConfirmPassword { get; set; }
        public string? Address { get; set; }
        public long? Point { get; set; }
        public bool? Deleted { get; set; }


    }
}
