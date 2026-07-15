using ShipmentApi.Abstractions;
using ShipmentApi.Domain;

namespace ShipmentApi.Infrastructure;

/// <summary>
/// Minimal in-memory catalog so the running API and integration tests have some product
/// data to work with. Not part of the demo anchors - just enough to make Program.cs real.
/// </summary>
public sealed class InMemoryProductCatalog : IProductCatalog
{
    private static readonly Dictionary<string, Product> Products = new()
    {
        ["AMBIENT-1"] = new Product("AMBIENT-1", StorageClass.Ambient),
        ["CHILLED-1"] = new Product("CHILLED-1", StorageClass.Chilled),
        ["FROZEN-1"] = new Product("FROZEN-1", StorageClass.Frozen),
    };

    public Task<Product> GetAsync(string productCode, CancellationToken cancellationToken = default) =>
        Products.TryGetValue(productCode, out var product)
            ? Task.FromResult(product)
            : throw new KeyNotFoundException($"Product '{productCode}' was not found in the catalog.");
}
