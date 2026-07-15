namespace ShipmentApi.UnitTests;

[TestClass]
public sealed class ShipmentServiceValidationTests
{
    private readonly IShipmentRepository _shipments = Substitute.For<IShipmentRepository>();
    private readonly IDeliveryLocationRepository _locations = Substitute.For<IDeliveryLocationRepository>();
    private readonly IProductCatalog _catalog = Substitute.For<IProductCatalog>();
    private readonly INotificationSender _notifications = Substitute.For<INotificationSender>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero));
    private readonly FakeLogger<ShipmentService> _logger = new();
    private readonly ShipmentService _sut;

    public ShipmentServiceValidationTests()
    {
        _sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, _timeProvider, _logger);
    }

    [TestMethod]
    [DataRow(0, DisplayName = "Quantity of zero is rejected")]
    [DataRow(-1)]
    [DataRow(501)]
    public async Task BookAsync_QuantityOutOfRange_ThrowsArgumentOutOfRangeException(int quantity)
    {
        // Arrange
        var request = new BookShipmentRequest(42, "AMBIENT-1", quantity);

        // Act
        var bookingTask = _sut.BookAsync(request);

        // Assert
        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => bookingTask);
    }

    [TestMethod]
    [DynamicData(nameof(ColdChainMatrix))]
    public async Task BookAsync_ColdChainMatrix_MatchesExpectedOutcome(bool needsColdChain, bool locationHasColdChainStorage, bool isAccepted)
    {
        // Arrange
        var storageClass = needsColdChain ? StorageClass.Frozen : StorageClass.Ambient;
        var location = new DeliveryLocation(42, "Warehouse", true, locationHasColdChainStorage);
        var product = new Product("SKU-1", storageClass);
        var request = new BookShipmentRequest(42, "SKU-1", 10);

        _locations.GetAsync(42).Returns(location);
        _catalog.GetAsync("SKU-1").Returns(product);
        _shipments.FindExistingAsync(42, "SKU-1", Arg.Any<DateOnly>()).Returns((Shipment?)null);

        // Act
        var bookingTask = _sut.BookAsync(request);

        // Assert
        if (isAccepted)
        {
            var result = await bookingTask;
            Assert.AreEqual(ShipmentStatus.Booked, result.Status);
        }
        else
        {
            await Assert.ThrowsExactlyAsync<ColdChainNotSupportedException>(() => bookingTask);
        }
    }

    public static IEnumerable<object[]> ColdChainMatrix =>
        new List<object[]>
        {
            // needsColdChain, locationHasColdChainStorage, isAccepted
            new object[] { false, false, true },
            new object[] { false, true, true },
            new object[] { true, true, true },
            new object[] { true, false, false },
        };
}
