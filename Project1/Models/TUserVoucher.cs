using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TUserVoucher
{
    public long UserId { get; set; }

    public long VoucherId { get; set; }

    public bool? Deleted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public virtual TUser User { get; set; } = null!;

    public virtual TVoucher Voucher { get; set; } = null!;
}
