using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TCategory
{
    public long CategoryId { get; set; }

    public string? Name { get; set; }

    public bool? Deleted { get; set; }

    public string? Image { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public virtual ICollection<TProduct> TProducts { get; set; } = new List<TProduct>();
}
