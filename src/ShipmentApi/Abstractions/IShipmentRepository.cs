using ShipmentApi.Domain;

namespace ShipmentApi.Abstractions;

public interface IShipmentRepository
{
    Task<Shipment?> FindExistingAsync(
        int locationId,
        string productCode,
        DateOnly dispatchDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
}
