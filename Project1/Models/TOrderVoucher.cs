using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TOrderVoucher
{
    public long OrderId { get; set; }

    public long VoucherId { get; set; }

    public bool? Deleted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
