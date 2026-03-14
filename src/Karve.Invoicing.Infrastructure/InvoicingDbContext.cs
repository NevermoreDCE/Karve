using Microsoft.EntityFrameworkCore;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Infrastructure;

public class InvoicingDbContext : DbContext
{
    private static readonly IReadOnlyList<Guid> EmptyCompanyIds = Array.Empty<Guid>();
    private readonly ICurrentUserService? _currentUser;

    public InvoicingDbContext(
        DbContextOptions<InvoicingDbContext> options,
        ICurrentUserService? currentUser = null) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<CompanyUser> CompanyUsers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    private IReadOnlyList<Guid> CurrentCompanyIds =>
        _currentUser?.CompanyIds ?? EmptyCompanyIds;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure owned types
        modelBuilder.Entity<Product>().OwnsOne(p => p.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<InvoiceLineItem>().OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<Payment>().OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<AppUser>().OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").IsRequired();
        });

        modelBuilder.Entity<Customer>().OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").IsRequired();
        });

        // Global tenant filters. If there is no authenticated user/company membership,
        // CurrentCompanyIds is empty and these predicates return no rows.
        modelBuilder.Entity<Company>()
            .HasQueryFilter(e => CurrentCompanyIds.Contains(e.Id));

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(e => CurrentCompanyIds.Contains(e.CompanyId));

        modelBuilder.Entity<Product>()
            .HasQueryFilter(e => CurrentCompanyIds.Contains(e.CompanyId));

        modelBuilder.Entity<Invoice>()
            .HasQueryFilter(e => CurrentCompanyIds.Contains(e.CompanyId));

        modelBuilder.Entity<InvoiceLineItem>()
            .HasQueryFilter(e => CurrentCompanyIds.Contains(e.Invoice.CompanyId));

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(e => CurrentCompanyIds.Contains(e.CompanyId));

        modelBuilder.Entity<CompanyUser>()
            .HasQueryFilter(e => CurrentCompanyIds.Contains(e.CompanyId));

        // Configure relationships
        modelBuilder.Entity<CompanyUser>()
            .HasKey(cu => new { cu.CompanyId, cu.UserId });

        modelBuilder.Entity<CompanyUser>()
            .HasOne(cu => cu.Company)
            .WithMany(c => c.CompanyUsers)
            .HasForeignKey(cu => cu.CompanyId);

        modelBuilder.Entity<CompanyUser>()
            .HasOne(cu => cu.User)
            .WithMany(u => u.CompanyUsers)
            .HasForeignKey(cu => cu.UserId);

        // One-to-many relationships
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Company)
            .WithMany(c => c.Customers)
            .HasForeignKey(c => c.CompanyId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Company)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CompanyId);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Company)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CompanyId);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId);

        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(li => li.Invoice)
            .WithMany(i => i.LineItems)
            .HasForeignKey(li => li.InvoiceId);

        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(li => li.Product)
            .WithMany(p => p.LineItems)
            .HasForeignKey(li => li.ProductId);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Company)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.CompanyId);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId);

        // Configure table names to camelCase
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                entity.SetTableName(tableName.ToLowerInvariant());
            }
        }

        // Configure required fields
        modelBuilder.Entity<Company>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.ExternalUserId)
            .IsRequired();

        modelBuilder.Entity<Customer>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<Customer>()
            .Property(c => c.BillingAddress)
            .IsRequired();

        modelBuilder.Entity<Product>()
            .Property(p => p.Name)
            .IsRequired();

        modelBuilder.Entity<Product>()
            .Property(p => p.Sku)
            .IsRequired();

        modelBuilder.Entity<Invoice>()
            .Property(i => i.InvoiceDate)
            .IsRequired();

        modelBuilder.Entity<Invoice>()
            .Property(i => i.DueDate)
            .IsRequired();

        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentDate)
            .IsRequired();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Method)
            .IsRequired();
    }
}