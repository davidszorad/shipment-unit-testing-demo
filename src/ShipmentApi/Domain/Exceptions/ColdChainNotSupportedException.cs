namespace ShipmentApi.Domain.Exceptions;

public sealed class ColdChainNotSupportedException : Exception
{
    public ColdChainNotSupportedException()
    {
    }

    public ColdChainNotSupportedException(string message)
        : base(message)
    {
    }

    public ColdChainNotSupportedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ColdChainNotSupportedException(string productCode, int locationId)
        : base($"Product '{productCode}' requires cold-chain storage, but location {locationId} does not have it.")
    {
    }
}
