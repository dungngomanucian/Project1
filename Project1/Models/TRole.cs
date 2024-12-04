using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TRole
{
    public long RoleId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public virtual ICollection<TUser> TUsers { get; set; } = new List<TUser>();
}
