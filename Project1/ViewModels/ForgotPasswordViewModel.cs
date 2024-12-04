using System.ComponentModel.DataAnnotations;

namespace Project1.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
    }
}
