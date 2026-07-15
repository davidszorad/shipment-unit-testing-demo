// DEMO D1: this class has zero seams. Do not fix it.
using ShipmentApi.Domain;
using ShipmentApi.Domain.Exceptions;
using ShipmentApi.Infrastructure;

namespace ShipmentApi.Services.Legacy;

/// <summary>
/// Implements the exact same booking rules as <see cref="ShipmentService"/>, but every
/// dependency is constructed (or read) directly inside the method instead of being
/// injected. There is no way to unit test this class - see
/// tests/ShipmentApi.UnitTests/Legacy/LegacyShipmentServiceTests.cs for why.
/// </summary>
public sealed class LegacyShipmentService
{
    public Shipment Book(BookShipmentRequest request)
    {
        if (request.Quantity is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.Quantity,
                "Quantity must be between 1 and 500 inclusive.");
        }

        using var dbContext = new ShipmentDbContext();

        var location = dbContext.DeliveryLocations.SingleOrDefault(l => l.Id == request.LocationId)
            ?? throw new LocationNotFoundException(request.LocationId);

        if (!location.IsActive)
        {
            throw new LocationInactiveException(request.LocationId);
        }

        var requiresColdChain = LegacyConfiguration.Instance.ColdChainProductCodes.Contains(request.ProductCode);
        if (requiresColdChain && !location.HasColdChainStorage)
        {
            throw new ColdChainNotSupportedException(request.ProductCode, request.LocationId);
        }

        var dispatchDate = DispatchWindow.Calculate(DateTime.UtcNow);

        var existingShipment = dbContext.Shipments.SingleOrDefault(s =>
            s.LocationId == request.LocationId
            && s.ProductCode == request.ProductCode
            && s.DispatchDate == dispatchDate);

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

        dbContext.Shipments.Add(shipment);
        dbContext.SaveChanges();

        var notificationSender = new SmtpNotificationSender("smtp.corp.local");
        var confirmation = new BookingConfirmation(
            shipment.LocationId,
            shipment.ProductCode,
            shipment.Quantity,
            shipment.DispatchDate,
            shipment.Id);

        notificationSender.SendBookingConfirmationAsync(confirmation).GetAwaiter().GetResult();

        return shipment;
    }
}
