using ShipmentApi.Domain;

namespace ShipmentApi.Abstractions;

public interface IProductCatalog
{
    Task<Product> GetAsync(string productCode, CancellationToken cancellationToken = default);
}
