namespace ShipmentApi.Domain;

public sealed record BookingConfirmation(
    int LocationId,
    string ProductCode,
    int Quantity,
    DateOnly DispatchDate,
    Guid ShipmentId);
