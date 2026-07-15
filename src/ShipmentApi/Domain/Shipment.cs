namespace ShipmentApi.Domain;

public sealed record Shipment(
    Guid Id,
    int LocationId,
    string ProductCode,
    int Quantity,
    DateOnly DispatchDate,
    ShipmentStatus Status);
