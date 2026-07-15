using ShipmentApi.Domain;

namespace ShipmentApi.Abstractions;

public interface INotificationSender
{
    Task SendBookingConfirmationAsync(BookingConfirmation confirmation, CancellationToken cancellationToken = default);
}
