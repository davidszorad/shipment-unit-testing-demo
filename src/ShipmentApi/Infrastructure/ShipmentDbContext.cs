using Microsoft.EntityFrameworkCore;
using ShipmentApi.Domain;

namespace ShipmentApi.Infrastructure;

public class ShipmentDbContext : DbContext
{
    /// <summary>
    /// Parameterless constructor used ONLY by <see cref="Services.Legacy.LegacyShipmentService"/>
    /// to demonstrate a dependency that is impossible to substitute in a test - it always
    /// points at a real SQLite file on disk.
    /// </summary>
    public ShipmentDbContext()
    {
    }

    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Shipment> Shipments => Set<Shipment>();

    public DbSet<DeliveryLocation> DeliveryLocations => Set<DeliveryLocation>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=shipments-legacy.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.LocationId, s.ProductCode, s.DispatchDate }).IsUnique();
        });

        modelBuilder.Entity<DeliveryLocation>(entity =>
        {
            entity.HasKey(l => l.Id);
        });
    }
}
