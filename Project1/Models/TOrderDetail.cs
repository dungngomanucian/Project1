using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TOrderDetail
{
    public long OrderId { get; set; }

    public long ProductDetailId { get; set; }

    public long? Number { get; set; }

    public string? Name { get; set; }

    public bool? Deleted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public virtual TOrder Order { get; set; } = null!;

    public virtual TProductDetail ProductDetail { get; set; } = null!;
}
