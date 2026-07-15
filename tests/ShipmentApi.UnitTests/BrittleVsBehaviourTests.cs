namespace ShipmentApi.UnitTests;

[TestClass]
public sealed class BrittleVsBehaviourTests
{
    private readonly IShipmentRepository _shipments = Substitute.For<IShipmentRepository>();
    private readonly IDeliveryLocationRepository _locations = Substitute.For<IDeliveryLocationRepository>();
    private readonly IProductCatalog _catalog = Substitute.For<IProductCatalog>();
    private readonly INotificationSender _notifications = Substitute.For<INotificationSender>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero));
    private readonly FakeLogger<ShipmentService> _logger = new();
    private readonly ShipmentService _sut;

    public BrittleVsBehaviourTests()
    {
        _sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, _timeProvider, _logger);
        var location = new DeliveryLocation(42, "Warehouse North", true, false);
        var product = new Product("AMBIENT-1", StorageClass.Ambient);

        _locations.GetAsync(42).Returns(location);
        _catalog.GetAsync("AMBIENT-1").Returns(product);
        _shipments.FindExistingAsync(42, "AMBIENT-1", Arg.Any<DateOnly>()).Returns((Shipment?)null);
    }

    // DEMO D7: BRITTLE - asserts HOW, not WHAT.
    [TestMethod]
    public async Task BookAsync_ValidRequest_BrittleImplementationCoupledTest()
    {
        // Arrange
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);

        // Act
        await _sut.BookAsync(request);

        // Assert
        Received.InOrder(() =>
        {
            _locations.GetAsync(42);
            _shipments.FindExistingAsync(42, "AMBIENT-1", Arg.Any<DateOnly>());
            _shipments.AddAsync(Arg.Any<Shipment>());
            _notifications.SendBookingConfirmationAsync(Arg.Any<BookingConfirmation>());
        });
        await _locations.Received(1).GetAsync(42);
        await _catalog.Received(1).GetAsync("AMBIENT-1");
        Assert.HasCount(1, _logger.Collector.GetSnapshot());
    }

    // DEMO D7: GOOD - asserts WHAT the outside world observes.
    [TestMethod]
    public async Task BookAsync_ValidRequest_BehaviourTest()
    {
        // Arrange
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);

        // Act
        var result = await _sut.BookAsync(request);

        // Assert
        Assert.AreEqual(ShipmentStatus.Booked, result.Status);
        Assert.AreEqual(new DateOnly(2026, 7, 13), result.DispatchDate);
        await _notifications.Received(1).SendBookingConfirmationAsync(Arg.Any<BookingConfirmation>());
    }
}
