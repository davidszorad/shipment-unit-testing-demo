namespace ShipmentApi.Domain;

public sealed record BookShipmentRequest(int LocationId, string ProductCode, int Quantity);
