using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project1.Models;

public partial class TUser
{
    public long UserId { get; set; }

    public long? RoleId { get; set; }

    public string? GoogleId { get; set; }

    public string? Code { get; set; }

    public string? Nickname { get; set; }


    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public long? Point { get; set; }

    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải là 10 chữ số")]
    public string? PhoneNumber { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public bool RememberMe { get; set; }

    public virtual TRole? Role { get; set; }

    public virtual ICollection<TOrder> TOrderCustomers { get; set; } = new List<TOrder>();

    public virtual ICollection<TOrder> TOrderManagers { get; set; } = new List<TOrder>();
}
