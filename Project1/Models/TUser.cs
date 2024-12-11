using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TUser
{
    public long UserId { get; set; }

    public long? RoleId { get; set; }

    public string? GoogleId { get; set; }

    public string? Code { get; set; }

    public string? Nickname { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public long? Point { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public bool? IsGoogleAccount { get; set; }

    public bool RememberMe { get; set; }

    public virtual TRole? Role { get; set; }

    public virtual ICollection<TOrder> TOrderCustomers { get; set; } = new List<TOrder>();

    public virtual ICollection<TOrder> TOrderManagers { get; set; } = new List<TOrder>();
}
