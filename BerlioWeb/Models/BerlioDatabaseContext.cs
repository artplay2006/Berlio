using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BerlioWeb.Models;

public partial class BerlioDatabaseContext : DbContext
{// Scaffold-DbContext "Host=localhost;Port=5432;Database=BerlioDatabase;Username=postgres;Password=postgres" Npgsql.EntityFrameworkCore.PostgreSQL -OutputDir Models -Force
    public BerlioDatabaseContext()
    {
    }

    public BerlioDatabaseContext(DbContextOptions<BerlioDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BerlioDatabase;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Equipment_pkey");

            entity.HasIndex(e => e.Name, "Equipment_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.LongDescription).HasColumnName("long description");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ShortDescription).HasColumnName("short description");
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Programs_pkey");

            entity.HasIndex(e => e.Name, "Programs_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.LongDescription).HasColumnName("long description");
            entity.Property(e => e.Name).HasColumnName("name");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
