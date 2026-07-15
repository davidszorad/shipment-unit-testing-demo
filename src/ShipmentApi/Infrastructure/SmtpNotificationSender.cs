using ShipmentApi.Abstractions;
using ShipmentApi.Domain;

namespace ShipmentApi.Infrastructure;

/// <summary>
/// A sealed, concrete class with non-virtual methods. This is deliberate: NSubstitute
/// cannot create a proxy for a sealed class, so `Substitute.For&lt;SmtpNotificationSender&gt;()`
/// fails at runtime. Substituting a concrete class instead of an interface either fails
/// outright (sealed) or silently does nothing useful (non-virtual members on a non-sealed
/// class are never intercepted). Depend on <see cref="INotificationSender"/> instead.
/// </summary>
public sealed class SmtpNotificationSender(string smtpHost) : INotificationSender
{
    public Task SendBookingConfirmationAsync(BookingConfirmation confirmation, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"No SMTP server reachable at '{smtpHost}' in this demo environment.");
}
