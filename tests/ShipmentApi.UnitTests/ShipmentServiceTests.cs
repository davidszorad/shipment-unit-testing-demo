namespace ShipmentApi.UnitTests;

[TestClass]
public sealed class ShipmentServiceTests
{
    private readonly IShipmentRepository _shipments = Substitute.For<IShipmentRepository>();
    private readonly IDeliveryLocationRepository _locations = Substitute.For<IDeliveryLocationRepository>();
    private readonly IProductCatalog _catalog = Substitute.For<IProductCatalog>();
    private readonly INotificationSender _notifications = Substitute.For<INotificationSender>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero));
    private readonly FakeLogger<ShipmentService> _logger = new();
    private readonly ShipmentService _sut;

    public ShipmentServiceTests()
    {
        _sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, _timeProvider, _logger);
    }

    [TestMethod]
    public async Task BookAsync_ValidRequest_ReturnsBookedShipment()
    {
        // Arrange
        var location = new DeliveryLocation(42, "Warehouse North", true, false);
        var product = new Product("AMBIENT-1", StorageClass.Ambient);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);

        _locations.GetAsync(42).Returns(location);
        _catalog.GetAsync("AMBIENT-1").Returns(product);
        _shipments.FindExistingAsync(42, "AMBIENT-1", Arg.Any<DateOnly>()).Returns((Shipment?)null);

        // Act
        var result = await _sut.BookAsync(request);

        // Assert
        Assert.AreEqual(ShipmentStatus.Booked, result.Status);
    }

    [TestMethod]
    public async Task BookAsync_ValidRequest_SendsExactlyOneConfirmation()
    {
        // Arrange
        var location = new DeliveryLocation(42, "Warehouse North", true, false);
        var product = new Product("AMBIENT-1", StorageClass.Ambient);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);

        _locations.GetAsync(42).Returns(location);
        _catalog.GetAsync("AMBIENT-1").Returns(product);
        _shipments.FindExistingAsync(42, "AMBIENT-1", Arg.Any<DateOnly>()).Returns((Shipment?)null);

        // Act
        await _sut.BookAsync(request);

        // Assert
        await _notifications.Received(1).SendBookingConfirmationAsync(
            Arg.Is<BookingConfirmation>(c => c.LocationId == 42));
    }

    [TestMethod]
    public async Task BookAsync_UnknownLocation_ThrowsLocationNotFound()
    {
        // Arrange
        var request = new BookShipmentRequest(99, "AMBIENT-1", 10);

        _locations.GetAsync(99).Returns((DeliveryLocation?)null);

        // Act
        var bookingTask = _sut.BookAsync(request);

        // Assert
        await Assert.ThrowsExactlyAsync<LocationNotFoundException>(() => bookingTask);
    }

    [TestMethod]
    public async Task BookAsync_InactiveLocation_ThrowsLocationInactive()
    {
        // Arrange
        var location = new DeliveryLocation(7, "Closed Depot", false, false);
        var request = new BookShipmentRequest(7, "AMBIENT-1", 10);

        _locations.GetAsync(7).Returns(location);

        // Act
        var bookingTask = _sut.BookAsync(request);

        // Assert
        await Assert.ThrowsExactlyAsync<LocationInactiveException>(() => bookingTask);
    }

    [TestMethod]
    public async Task BookAsync_ColdChainProductAtWarmLocation_ThrowsColdChainNotSupported()
    {
        // Arrange
        var location = new DeliveryLocation(3, "Dry Store", true, false);
        var product = new Product("FROZEN-1", StorageClass.Frozen);
        var request = new BookShipmentRequest(3, "FROZEN-1", 10);

        _locations.GetAsync(3).Returns(location);
        _catalog.GetAsync("FROZEN-1").Returns(product);

        // Act
        var bookingTask = _sut.BookAsync(request);

        // Assert
        await Assert.ThrowsExactlyAsync<ColdChainNotSupportedException>(() => bookingTask);
    }

    [TestMethod]
    public async Task BookAsync_ColdChainProductAtWarmLocation_DoesNotSendNotification()
    {
        // Arrange
        var location = new DeliveryLocation(3, "Dry Store", true, false);
        var product = new Product("FROZEN-1", StorageClass.Frozen);
        var request = new BookShipmentRequest(3, "FROZEN-1", 10);

        _locations.GetAsync(3).Returns(location);
        _catalog.GetAsync("FROZEN-1").Returns(product);

        // Act
        var bookingTask = _sut.BookAsync(request);
        await Assert.ThrowsExactlyAsync<ColdChainNotSupportedException>(() => bookingTask);

        // Assert
        await _notifications.DidNotReceive().SendBookingConfirmationAsync(Arg.Any<BookingConfirmation>());
    }

    [TestMethod]
    public async Task BookAsync_DuplicateBookingSameDay_ReturnsExistingAndDoesNotNotify()
    {
        // Arrange
        var location = new DeliveryLocation(42, "Warehouse North", true, false);
        var product = new Product("AMBIENT-1", StorageClass.Ambient);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);
        var existingShipment = new Shipment(Guid.NewGuid(), 42, "AMBIENT-1", 10, new DateOnly(2026, 7, 13), ShipmentStatus.Booked);

        _locations.GetAsync(42).Returns(location);
        _catalog.GetAsync("AMBIENT-1").Returns(product);
        _shipments.FindExistingAsync(42, "AMBIENT-1", Arg.Any<DateOnly>()).Returns(existingShipment);

        // Act
        var result = await _sut.BookAsync(request);

        // Assert
        Assert.AreEqual(existingShipment, result);
        await _shipments.DidNotReceive().AddAsync(Arg.Any<Shipment>());
        await _notifications.DidNotReceive().SendBookingConfirmationAsync(Arg.Any<BookingConfirmation>());
    }

    [TestMethod]
    public async Task BookAsync_ValidRequest_CapturesShipmentPassedToRepository()
    {
        // Arrange
        var location = new DeliveryLocation(42, "Warehouse North", true, false);
        var product = new Product("AMBIENT-1", StorageClass.Ambient);
        var request = new BookShipmentRequest(42, "AMBIENT-1", 250);
        Shipment? capturedShipment = null;

        _locations.GetAsync(42).Returns(location);
        _catalog.GetAsync("AMBIENT-1").Returns(product);
        _shipments.FindExistingAsync(42, "AMBIENT-1", Arg.Any<DateOnly>()).Returns((Shipment?)null);
        _shipments.AddAsync(Arg.Do<Shipment>(s => capturedShipment = s)).Returns(Task.CompletedTask);

        // Act
        await _sut.BookAsync(request);

        // Assert
        Assert.IsNotNull(capturedShipment);
        Assert.AreEqual(250, capturedShipment.Quantity);
    }
}
