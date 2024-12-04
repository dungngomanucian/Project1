using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TCombo
{
    public long ComboId { get; set; }

    public string? Name { get; set; }

    public string? Image { get; set; }

    public double? Price { get; set; }

    public bool? Deleted { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public virtual ICollection<TOrderCombo> TOrderCombos { get; set; } = new List<TOrderCombo>();

    public virtual ICollection<TProductDetailCombo> TProductDetailCombos { get; set; } = new List<TProductDetailCombo>();
}
