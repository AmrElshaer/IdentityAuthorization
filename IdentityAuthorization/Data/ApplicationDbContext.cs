using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityAuthorization.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):IdentityDbContext<ApplicationUser>(options)
{
    // Domain entities
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // this base for identity tables
        base.OnModelCreating(builder);
        
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.EnableNotifications).HasDefaultValue(true);
        });
        
        // Configure Order entity
        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders", "Domain");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.CustomerId)
                .IsRequired()
                .HasMaxLength(450); // Same as Identity UserId
            
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);
            
            // Index for order number (unique)
            entity.HasIndex(e => e.Number)
                .IsUnique()
                .HasDatabaseName("IX_Orders_Number");
            
            // Index for customer lookups
            entity.HasIndex(e => e.CustomerId)
                .HasDatabaseName("IX_Orders_CustomerId");
        });
        
        // Configure Contract entity
        builder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contracts", "Domain");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);
            
            // Index for contract number (unique)
            entity.HasIndex(e => e.Number)
                .IsUnique()
                .HasDatabaseName("IX_Contracts_Number");
        });
        
        // Configure Inventory entity
        builder.Entity<Inventory>(entity =>
        {
            entity.ToTable("Inventories", "Domain");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ProductId)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.InitialQuantity)
                .IsRequired();
            
            entity.Property(e => e.CurrentQuantity)
                .IsRequired();
            
            entity.Property(e => e.Location)
                .HasMaxLength(200);
            
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);
            
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(450);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);
            
            // Index for product lookups (unique per product)
            entity.HasIndex(e => e.ProductId)
                .IsUnique()
                .HasDatabaseName("IX_Inventories_ProductId");
            
            // Index for low stock queries
            entity.HasIndex(e => e.CurrentQuantity)
                .HasDatabaseName("IX_Inventories_CurrentQuantity");
            
            // Index for location-based queries
            entity.HasIndex(e => e.Location)
                .HasDatabaseName("IX_Inventories_Location");
            
            // Index for audit queries
            entity.HasIndex(e => e.CreatedBy)
                .HasDatabaseName("IX_Inventories_CreatedBy");
        });
        
        builder.HasDefaultSchema("Identity");
    }
}

public sealed class ApplicationUser:IdentityUser
{
    public bool EnableNotifications { get; set; }
}