namespace ShipmentApi.Domain;

/// <summary>
/// Pure business logic, zero dependencies. This is why D3 needs no mocks.
/// </summary>
public static class ColdChainRules
{
    public static bool RequiresColdChain(Product product) =>
        product.StorageClass is StorageClass.Chilled or StorageClass.Frozen;
}
