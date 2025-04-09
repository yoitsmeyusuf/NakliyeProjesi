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

    // Shipment - Bid ilişkisini açıkça tanımla
    modelBuilder.Entity<Bid>()
        .HasOne(b => b.Shipment)
        .WithMany(s => s.Bids)
        .HasForeignKey(b => b.ShipmentId)
        .OnDelete(DeleteBehavior.Cascade);
}

}
