using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TVoucher
{
    public long VoucherId { get; set; }

    public string? Code { get; set; }

    public long? Number { get; set; }

    public bool? IsPercentDiscountType { get; set; }

    public double? MinOrderValue { get; set; }

    public double? DiscountValue { get; set; }

    public double? MaxDiscountValue { get; set; }

    public string? ApplyCondition { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public bool? Deleted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public virtual ICollection<TOrder> TOrders { get; set; } = new List<TOrder>();
}
