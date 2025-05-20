using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BerlioWeb.Models;

public partial class BerlioDatabaseContext : DbContext
{
    public BerlioDatabaseContext()
    {
    }

    public BerlioDatabaseContext(DbContextOptions<BerlioDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BalancesOfService> BalancesOfServices { get; set; }

    public virtual DbSet<DepositHistory> DepositHistories { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<EquipmentDelivery> EquipmentDeliveries { get; set; }

    public virtual DbSet<OrderSell> OrderSells { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UsersBankCard> UsersBankCards { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BerlioDatabase;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BalancesOfService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BalancesOfServices_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.Loginclient).HasColumnName("loginclient");
            entity.Property(e => e.Nameservice).HasColumnName("nameservice");

            entity.HasOne(d => d.LoginclientNavigation).WithMany(p => p.BalancesOfServices)
                .HasForeignKey(d => d.Loginclient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BalancesOfServices_loginclient_fkey");
        });

        modelBuilder.Entity<DepositHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DepositHistory_pkey");

            entity.ToTable("DepositHistory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Loginclient).HasColumnName("loginclient");
            entity.Property(e => e.Sumofmoney).HasColumnName("sumofmoney");
            entity.Property(e => e.Timedeposit)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timedeposit");
            entity.Property(e => e.TypeBalance).HasColumnName("type balance");

            entity.HasOne(d => d.LoginclientNavigation).WithMany(p => p.DepositHistories)
                .HasForeignKey(d => d.Loginclient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DepositHistory_loginclient_fkey1");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Equipment_pkey");

            entity.HasIndex(e => e.Name, "Equipment_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Countavailability).HasColumnName("countavailability");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.LongDescription).HasColumnName("long description");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ShortDescription).HasColumnName("short description");
        });

        modelBuilder.Entity<EquipmentDelivery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EquipmentDelivery_pkey");

            entity.ToTable("EquipmentDelivery");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Addressdelivery).HasColumnName("addressdelivery");
            entity.Property(e => e.Idordersell).HasColumnName("idordersell");
            entity.Property(e => e.Timedelivery)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timedelivery");

            entity.HasOne(d => d.IdordersellNavigation).WithMany(p => p.EquipmentDeliveries)
                .HasForeignKey(d => d.Idordersell)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EquipmentDelivery_idordersell_fkey");
        });

        modelBuilder.Entity<OrderSell>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OrderSell_pkey");

            entity.ToTable("OrderSell");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Client).HasColumnName("client");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Finished)
                .HasDefaultValue(false)
                .HasColumnName("finished");
            entity.Property(e => e.Idproduct).HasColumnName("idproduct");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Programs_pkey");

            entity.HasIndex(e => e.Name, "Programs_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.LongDescription).HasColumnName("long description");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Pathtodownload).HasColumnName("pathtodownload");
            entity.Property(e => e.ShortDescription).HasColumnName("short description");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Login).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.ContractNumber).HasColumnName("contract number");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.PlaceOfTheContract).HasColumnName("place of the contract");
            entity.Property(e => e.Telephone).HasColumnName("telephone");
        });

        modelBuilder.Entity<UsersBankCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BankCardDetails_pkey");

            entity.ToTable("UsersBankCard");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cvc).HasColumnName("cvc");
            entity.Property(e => e.Loginclient).HasColumnName("loginclient");
            entity.Property(e => e.Numcard).HasColumnName("numcard");
            entity.Property(e => e.ValidityPeriod).HasColumnName("validity period");

            entity.HasOne(d => d.LoginclientNavigation).WithMany(p => p.UsersBankCards)
                .HasForeignKey(d => d.Loginclient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UsersBankCard_loginclient_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
