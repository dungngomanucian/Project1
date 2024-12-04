using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TProductDetail
{
    public long ProductDetailId { get; set; }

    public long? Number { get; set; }

    public double? Price { get; set; }

    public bool? Deleted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public long? ProductId { get; set; }

    public long? SizeId { get; set; }

    public long? CrustId { get; set; }

    public virtual TCrust? Crust { get; set; }

    public virtual TProduct? Product { get; set; }

    public virtual TSize? Size { get; set; }

    public virtual ICollection<TOrderDetail> TOrderDetails { get; set; } = new List<TOrderDetail>();

    public virtual ICollection<TProductDetailCombo> TProductDetailCombos { get; set; } = new List<TProductDetailCombo>();
}
