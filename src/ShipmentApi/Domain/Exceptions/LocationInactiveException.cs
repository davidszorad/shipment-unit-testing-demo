namespace ShipmentApi.Domain.Exceptions;

public sealed class LocationInactiveException : Exception
{
    public LocationInactiveException()
    {
    }

    public LocationInactiveException(string message)
        : base(message)
    {
    }

    public LocationInactiveException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public LocationInactiveException(int locationId)
        : base($"Delivery location {locationId} is not active.")
    {
    }
}
