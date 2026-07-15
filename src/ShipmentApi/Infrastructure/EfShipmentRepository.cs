using Microsoft.EntityFrameworkCore;
using ShipmentApi.Abstractions;
using ShipmentApi.Domain;

namespace ShipmentApi.Infrastructure;

public sealed class EfShipmentRepository(ShipmentDbContext dbContext) : IShipmentRepository
{
    public async Task<Shipment?> FindExistingAsync(
        int locationId,
        string productCode,
        DateOnly dispatchDate,
        CancellationToken cancellationToken = default) =>
        await dbContext.Shipments.SingleOrDefaultAsync(
            s => s.LocationId == locationId && s.ProductCode == productCode && s.DispatchDate == dispatchDate,
            cancellationToken);

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
