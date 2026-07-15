namespace ShipmentApi.Domain;

public sealed record DeliveryLocation(int Id, string Name, bool IsActive, bool HasColdChainStorage);
