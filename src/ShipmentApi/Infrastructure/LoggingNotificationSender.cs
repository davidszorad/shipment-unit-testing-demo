using Microsoft.Extensions.Logging;
using ShipmentApi.Abstractions;
using ShipmentApi.Domain;

namespace ShipmentApi.Infrastructure;

/// <summary>
/// The real <see cref="INotificationSender"/> registered for the running API. Logs
/// instead of sending real email, which is all this demo needs.
/// </summary>
public sealed class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public Task SendBookingConfirmationAsync(BookingConfirmation confirmation, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Booking confirmation sent for shipment {ShipmentId}", confirmation.ShipmentId);
        return Task.CompletedTask;
    }
}
