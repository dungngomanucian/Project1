using System;
using System.Collections.Generic;

namespace Project1.Models;

public partial class TOrder
{
    public long OrderId { get; set; }

    public string? Code { get; set; }

    public DateTime? Date { get; set; }

    public string? Address { get; set; }

    public string? Note { get; set; }

    public string? CustomerFeeling { get; set; }

    public double? TotalPrice { get; set; }

    public bool? Deleted { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public long? PaymentMethodId { get; set; }

    public long? StatusId { get; set; }

    public long? CustomerId { get; set; }

    public long? ManagerId { get; set; }

    public long? VoucherId { get; set; }

    public virtual TUser? Customer { get; set; }

    public virtual TUser? Manager { get; set; }

    public virtual TPaymentMethod? PaymentMethod { get; set; }

    public virtual TStatus? Status { get; set; }

    public virtual ICollection<TOrderCombo> TOrderCombos { get; set; } = new List<TOrderCombo>();

    public virtual ICollection<TOrderDetail> TOrderDetails { get; set; } = new List<TOrderDetail>();

    public virtual TVoucher? Voucher { get; set; }
}
