using ShipmentApi.Domain;

namespace ShipmentApi.Abstractions;

public interface IDeliveryLocationRepository
{
    Task<DeliveryLocation?> GetAsync(int locationId, CancellationToken cancellationToken = default);
}
