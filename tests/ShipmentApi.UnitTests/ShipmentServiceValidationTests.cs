namespace ShipmentApi.UnitTests;

[TestClass]
public sealed class ShipmentServiceValidationTests
{
    public TestContext TestContext { get; set; } = null!;

    private IShipmentRepository _shipments = null!;
    private IDeliveryLocationRepository _locations = null!;
    private IProductCatalog _catalog = null!;
    private INotificationSender _notifications = null!;
    private FakeTimeProvider _timeProvider = null!;
    private FakeLogger<ShipmentService> _logger = null!;
    private ShipmentService _sut = null!;

    // DEMO: TestInitialize/TestCleanup - the "beforeEach"/"afterEach" of MSTest. MSTest
    // already creates a brand-new instance of this class per [TestMethod], so a
    // constructor would give every test the same fresh isolation; the main reason to
    // reach for TestInitialize instead is when setup needs TestContext (e.g. to log the
    // test name), which MSTest does not populate until after construction.
    [TestInitialize]
    public void SetUp()
    {
        _shipments = Substitute.For<IShipmentRepository>();
        _locations = Substitute.For<IDeliveryLocationRepository>();
        _catalog = Substitute.For<IProductCatalog>();
        _notifications = Substitute.For<INotificationSender>();
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero));
        _logger = new FakeLogger<ShipmentService>();
        _sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, _timeProvider, _logger);

        TestContext.WriteLine($"Starting {TestContext.TestName}");
    }

    [TestCleanup]
    public void TearDown()
    {
        TestContext.WriteLine($"Finished {TestContext.TestName}");
    }

    // DEMO: alternative - constructor instead of TestInitialize/TestCleanup. MSTest already
    // creates a fresh instance of this class per [TestMethod], so a constructor gives the
    // same per-test isolation with less ceremony. No IDisposable/IAsyncDisposable is needed
    // here because none of these fields hold an unmanaged resource (compare with
    // EfShipmentRepositoryTests, which does need IAsyncDisposable for its SQLite connection).
    // The one thing this alternative loses is the TestContext.WriteLine calls above. MSTest
    // does not populate the TestContext property until after the constructor runs, so
    // TestContext-dependent setup/logging is the one case that forces TestInitialize/
    // TestCleanup instead of a constructor. To demo this live: delete the SetUp/TearDown
    // methods (and the `= null!` field initializers) above, then paste this in.
    //
    // private readonly IShipmentRepository _shipments = Substitute.For<IShipmentRepository>();
    // private readonly IDeliveryLocationRepository _locations = Substitute.For<IDeliveryLocationRepository>();
    // private readonly IProductCatalog _catalog = Substitute.For<IProductCatalog>();
    // private readonly INotificationSender _notifications = Substitute.For<INotificationSender>();
    // private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero));
    // private readonly FakeLogger<ShipmentService> _logger = new();
    // private readonly ShipmentService _sut;
    //
    // public ShipmentServiceValidationTests()
    // {
    //     _sut = new ShipmentService(_shipments, _locations, _catalog, _notifications, _timeProvider, _logger);
    // }

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
