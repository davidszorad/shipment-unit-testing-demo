using Microsoft.Extensions.Logging;
using ShipmentApi.Abstractions;
using ShipmentApi.Domain;
using ShipmentApi.Domain.Exceptions;

namespace ShipmentApi.Services;

/// <summary>
/// The testable version of shipment booking. Every dependency is injected through an
/// abstraction (or <see cref="TimeProvider"/>), so every rule below can be unit tested
/// in isolation with NSubstitute - see D4 and D5.
/// </summary>
public sealed class ShipmentService(
    IShipmentRepository shipmentRepository,
    IDeliveryLocationRepository locationRepository,
    IProductCatalog productCatalog,
    INotificationSender notificationSender,
    TimeProvider timeProvider,
    ILogger<ShipmentService> logger)
{
    public async Task<Shipment> BookAsync(BookShipmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.Quantity,
                "Quantity must be between 1 and 500 inclusive.");
        }

        var location = await locationRepository.GetAsync(request.LocationId, cancellationToken)
            ?? throw new LocationNotFoundException(request.LocationId);

        if (!location.IsActive)
        {
            throw new LocationInactiveException(request.LocationId);
        }

        var product = await productCatalog.GetAsync(request.ProductCode, cancellationToken);

        if (ColdChainRules.RequiresColdChain(product) && !location.HasColdChainStorage)
        {
            throw new ColdChainNotSupportedException(request.ProductCode, request.LocationId);
        }

        var dispatchDate = DispatchWindow.Calculate(timeProvider.GetUtcNow());

        var existingShipment = await shipmentRepository.FindExistingAsync(
            request.LocationId,
            request.ProductCode,
            dispatchDate,
            cancellationToken);

        if (existingShipment is not null)
        {
            return existingShipment;
        }

        var shipment = new Shipment(
            Guid.NewGuid(),
            request.LocationId,
            request.ProductCode,
            request.Quantity,
            dispatchDate,
            ShipmentStatus.Booked);

        await shipmentRepository.AddAsync(shipment, cancellationToken);

        var confirmation = new BookingConfirmation(
            shipment.LocationId,
            shipment.ProductCode,
            shipment.Quantity,
            shipment.DispatchDate,
            shipment.Id);

        await notificationSender.SendBookingConfirmationAsync(confirmation, cancellationToken);

        logger.LogInformation(
            "Booked shipment {ShipmentId} for location {LocationId} dispatching {DispatchDate}",
            shipment.Id,
            shipment.LocationId,
            shipment.DispatchDate);

        return shipment;
    }
}
