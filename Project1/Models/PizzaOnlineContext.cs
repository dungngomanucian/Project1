using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Project1.Models;

public partial class PizzaOnlineContext : DbContext
{
    public PizzaOnlineContext()
    {
    }

    public PizzaOnlineContext(DbContextOptions<PizzaOnlineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TCategory> TCategories { get; set; }

    public virtual DbSet<TCombo> TCombos { get; set; }

    public virtual DbSet<TCrust> TCrusts { get; set; }

    public virtual DbSet<TOrder> TOrders { get; set; }

    public virtual DbSet<TOrderCombo> TOrderCombos { get; set; }

    public virtual DbSet<TOrderDetail> TOrderDetails { get; set; }

    public virtual DbSet<TPaymentMethod> TPaymentMethods { get; set; }

    public virtual DbSet<TProduct> TProducts { get; set; }

    public virtual DbSet<TProductDetail> TProductDetails { get; set; }

    public virtual DbSet<TProductDetailCombo> TProductDetailCombos { get; set; }

    public virtual DbSet<TRole> TRoles { get; set; }

    public virtual DbSet<TSize> TSizes { get; set; }

    public virtual DbSet<TStatus> TStatuses { get; set; }

    public virtual DbSet<TUser> TUsers { get; set; }

    public virtual DbSet<TVoucher> TVouchers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-CESR45S5\\SQLEXPRESS;Initial Catalog=PizzaOnline;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.ToTable("tCategory");

            entity.Property(e => e.CategoryId)
                .ValueGeneratedNever()
                .HasColumnName("Category_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.Image).HasMaxLength(50);
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);
        });

        modelBuilder.Entity<TCombo>(entity =>
        {
            entity.HasKey(e => e.ComboId);

            entity.ToTable("tCombo");

            entity.Property(e => e.ComboId)
                .ValueGeneratedNever()
                .HasColumnName("Combo_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.ExpirationDate)
                .HasColumnType("datetime")
                .HasColumnName("Expiration_date");
            entity.Property(e => e.Image).HasMaxLength(30);
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("Start_date");
        });

        modelBuilder.Entity<TCrust>(entity =>
        {
            entity.HasKey(e => e.CrustId);

            entity.ToTable("tCrust");

            entity.Property(e => e.CrustId)
                .ValueGeneratedNever()
                .HasColumnName("Crust_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);
        });

        modelBuilder.Entity<TOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId);

            entity.ToTable("tOrder");

            entity.Property(e => e.OrderId)
                .ValueGeneratedNever()
                .HasColumnName("Order_ID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.CustomerFeeling)
                .HasMaxLength(50)
                .HasColumnName("Customer_feeling");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_ID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.ManagerId).HasColumnName("Manager_ID");
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.PaymentMethodId).HasColumnName("PaymentMethod_ID");
            entity.Property(e => e.StatusId).HasColumnName("Status_ID");
            entity.Property(e => e.TotalPrice).HasColumnName("Total_price");
            entity.Property(e => e.VoucherId).HasColumnName("Voucher_ID");

            entity.HasOne(d => d.Customer).WithMany(p => p.TOrderCustomers)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_tOrder_tUser_Customer");

            entity.HasOne(d => d.Manager).WithMany(p => p.TOrderManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_tOrder_tUser_Manager");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.TOrders)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("FK_tOrder_tPaymentMethod");

            entity.HasOne(d => d.Status).WithMany(p => p.TOrders)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_tOrder_tStatus");

            entity.HasOne(d => d.Voucher).WithMany(p => p.TOrders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK_tOrder_tVoucher");
        });

        modelBuilder.Entity<TOrderCombo>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ComboId });

            entity.ToTable("tOrder_Combo");

            entity.Property(e => e.OrderId).HasColumnName("Order_ID");
            entity.Property(e => e.ComboId).HasColumnName("Combo_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");

            entity.HasOne(d => d.Combo).WithMany(p => p.TOrderCombos)
                .HasForeignKey(d => d.ComboId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tOrder_Combo_tCombo");

            entity.HasOne(d => d.Order).WithMany(p => p.TOrderCombos)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tOrder_Combo_tOrder");
        });

        modelBuilder.Entity<TOrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductDetailId });

            entity.ToTable("tOrderDetail");

            entity.Property(e => e.OrderId).HasColumnName("Order_ID");
            entity.Property(e => e.ProductDetailId).HasColumnName("ProductDetail_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);

            entity.HasOne(d => d.Order).WithMany(p => p.TOrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tOrderDetail_tOrder");

            entity.HasOne(d => d.ProductDetail).WithMany(p => p.TOrderDetails)
                .HasForeignKey(d => d.ProductDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tOrderDetail_tProductDetail");
        });

        modelBuilder.Entity<TPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId);

            entity.ToTable("tPaymentMethod");

            entity.Property(e => e.PaymentMethodId)
                .ValueGeneratedNever()
                .HasColumnName("PaymentMethod_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);
        });

        modelBuilder.Entity<TProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.ToTable("tProduct");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("Product_ID");
            entity.Property(e => e.CategoryId).HasColumnName("Category_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.Description).HasMaxLength(400);
            entity.Property(e => e.Image).HasMaxLength(50);
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.TProducts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_tProduct_tCategory");
        });

        modelBuilder.Entity<TProductDetail>(entity =>
        {
            entity.HasKey(e => e.ProductDetailId);

            entity.ToTable("tProductDetail");

            entity.Property(e => e.ProductDetailId)
                .ValueGeneratedNever()
                .HasColumnName("ProductDetail_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.CrustId).HasColumnName("Crust_ID");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.ProductId).HasColumnName("Product_ID");
            entity.Property(e => e.SizeId).HasColumnName("Size_ID");

            entity.HasOne(d => d.Crust).WithMany(p => p.TProductDetails)
                .HasForeignKey(d => d.CrustId)
                .HasConstraintName("FK_tProductDetail_tCrust");

            entity.HasOne(d => d.Product).WithMany(p => p.TProductDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_tProductDetail_tProduct");

            entity.HasOne(d => d.Size).WithMany(p => p.TProductDetails)
                .HasForeignKey(d => d.SizeId)
                .HasConstraintName("FK_tProductDetail_tSize");
        });

        modelBuilder.Entity<TProductDetailCombo>(entity =>
        {
            entity.HasKey(e => new { e.ProductDetailId, e.ComboId });

            entity.ToTable("tProductDetail_Combo");

            entity.Property(e => e.ProductDetailId).HasColumnName("ProductDetail_ID");
            entity.Property(e => e.ComboId).HasColumnName("Combo_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");

            entity.HasOne(d => d.Combo).WithMany(p => p.TProductDetailCombos)
                .HasForeignKey(d => d.ComboId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tProductDetail_Combo_tCombo");

            entity.HasOne(d => d.ProductDetail).WithMany(p => p.TProductDetailCombos)
                .HasForeignKey(d => d.ProductDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tProductDetail_Combo_tProductDetail");
        });

        modelBuilder.Entity<TRole>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.ToTable("tRole");

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("Role_ID");
            entity.Property(e => e.Code)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);
        });

        modelBuilder.Entity<TSize>(entity =>
        {
            entity.HasKey(e => e.SizeId);

            entity.ToTable("tSize");

            entity.Property(e => e.SizeId)
                .ValueGeneratedNever()
                .HasColumnName("Size_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);
        });

        modelBuilder.Entity<TStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId);

            entity.ToTable("tStatus");

            entity.Property(e => e.StatusId)
                .ValueGeneratedNever()
                .HasColumnName("Status_ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.Name).HasMaxLength(25);
        });

        modelBuilder.Entity<TUser>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("tUser");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_ID");
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.Code)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .HasColumnName("First_name");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(60)
                .HasColumnName("Google_ID");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.LastName)
                .HasMaxLength(20)
                .HasColumnName("Last_name");
            entity.Property(e => e.Nickname).HasMaxLength(25);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ResetToken).HasMaxLength(100);
            entity.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.RoleId).HasColumnName("Role_ID");

            entity.HasOne(d => d.Role).WithMany(p => p.TUsers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_tUser_tRole");
        });

        modelBuilder.Entity<TVoucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId);

            entity.ToTable("tVoucher");

            entity.Property(e => e.VoucherId)
                .ValueGeneratedNever()
                .HasColumnName("Voucher_ID");
            entity.Property(e => e.ApplyCondition)
                .HasMaxLength(400)
                .HasColumnName("Apply_Condition");
            entity.Property(e => e.Code)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("Created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_date");
            entity.Property(e => e.DiscountValue).HasColumnName("Discount_Value");
            entity.Property(e => e.ExpirationDate)
                .HasColumnType("datetime")
                .HasColumnName("Expiration_date");
            entity.Property(e => e.IsPercentDiscountType).HasColumnName("Is_Percent_Discount_Type");
            entity.Property(e => e.LastModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("Last_Modified_by");
            entity.Property(e => e.LastModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_Modified_date");
            entity.Property(e => e.MaxDiscountValue).HasColumnName("Max_Discount_Value");
            entity.Property(e => e.MinOrderValue).HasColumnName("Min_Order_Value");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("Start_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
