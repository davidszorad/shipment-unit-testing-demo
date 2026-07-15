using Microsoft.EntityFrameworkCore;
using ShipmentApi.Abstractions;
using ShipmentApi.Domain;

namespace ShipmentApi.Infrastructure;

public sealed class EfDeliveryLocationRepository(ShipmentDbContext dbContext) : IDeliveryLocationRepository
{
    public async Task<DeliveryLocation?> GetAsync(int locationId, CancellationToken cancellationToken = default) =>
        await dbContext.DeliveryLocations.SingleOrDefaultAsync(l => l.Id == locationId, cancellationToken);
}
