namespace NakliyeApp.Data;
using Microsoft.EntityFrameworkCore;
using NakliyeApp.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Rating> Ratings => Set<Rating>(); // Add Ratings DbSet

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.Shipments)
            .WithOne(s => s.Customer)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.Bids)
            .WithOne(b => b.Shipper)
            .HasForeignKey(b => b.ShipperId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bid>()
            .HasOne(b => b.Shipment)
            .WithMany(s => s.Bids)
            .HasForeignKey(b => b.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(n => n.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Rating relationships
        modelBuilder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>()
            .HasOne<Shipment>()
            .WithMany()
            .HasForeignKey(r => r.ShipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
