namespace ShipmentApi.UnitTests;

[TestClass]
public sealed class TimeAndLoggingTests
{
    private readonly IShipmentRepository _shipments = Substitute.For<IShipmentRepository>();
    private readonly IDeliveryLocationRepository _locations = Substitute.For<IDeliveryLocationRepository>();
    private readonly IProductCatalog _catalog = Substitute.For<IProductCatalog>();
    private readonly INotificationSender _notifications = Substitute.For<INotificationSender>();
    private readonly FakeLogger<ShipmentService> _logger = new();

    [TestMethod]
    public async Task BookAsync_FakeTimeAt0900Utc_DispatchesSameDay()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero));
        var sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, timeProvider, _logger);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);
        SetUpHappyPath(42, "AMBIENT-1");

        // Act
        var result = await sut.BookAsync(request);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 13), result.DispatchDate);
    }

    [TestMethod]
    public async Task BookAsync_FakeTimeAt1630Utc_DispatchesNextDay()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 7, 13, 16, 30, 0, TimeSpan.Zero));
        var sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, timeProvider, _logger);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);
        SetUpHappyPath(42, "AMBIENT-1");

        // Act
        var result = await sut.BookAsync(request);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 14), result.DispatchDate);
    }

    [TestMethod]
    public async Task BookAsync_FakeTimeFridayAt1630Utc_RollsToFollowingMonday()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 7, 17, 16, 30, 0, TimeSpan.Zero));
        var sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, timeProvider, _logger);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);
        SetUpHappyPath(42, "AMBIENT-1");

        // Act
        var result = await sut.BookAsync(request);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 20), result.DispatchDate);
    }

    [TestMethod]
    public async Task BookAsync_ValidRequest_LogsOneInformationRecord()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero));
        var sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, timeProvider, _logger);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);
        SetUpHappyPath(42, "AMBIENT-1");

        // Act
        await sut.BookAsync(request);

        // Assert
        var record = _logger.Collector.LatestRecord;
        Assert.AreEqual(LogLevel.Information, record.Level);
    }

    // DEMO D6: this test is here to show it is POSSIBLE, not that it is a good idea.
    // Logs are not behaviour. Do not assert on them in real tests.

    private void SetUpHappyPath(int locationId, string productCode)
    {
        var location = new DeliveryLocation(locationId, "Warehouse", true, false);
        var product = new Product(productCode, StorageClass.Ambient);

        _locations.GetAsync(locationId).Returns(location);
        _catalog.GetAsync(productCode).Returns(product);
        _shipments.FindExistingAsync(locationId, productCode, Arg.Any<DateOnly>()).Returns((Shipment?)null);
    }
}
