namespace ShipmentApi.Services.Legacy;

/// <summary>
/// A static configuration singleton, exactly the way a lot of legacy .NET code reads
/// settings: through a global, mutable, ambient object instead of an injected value.
/// </summary>
public sealed class LegacyConfiguration
{
    public static LegacyConfiguration Instance { get; } = new();

    public IReadOnlySet<string> ColdChainProductCodes { get; } = new HashSet<string> { "CHILLED-1", "FROZEN-1" };

    private LegacyConfiguration()
    {
    }
}
