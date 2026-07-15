namespace ShipmentApi.Domain.Exceptions;

public sealed class LocationNotFoundException : Exception
{
    public LocationNotFoundException()
    {
    }

    public LocationNotFoundException(string message)
        : base(message)
    {
    }

    public LocationNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public LocationNotFoundException(int locationId)
        : base($"Delivery location {locationId} was not found.")
    {
    }
}
