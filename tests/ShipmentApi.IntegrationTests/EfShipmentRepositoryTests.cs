using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ShipmentApi.IntegrationTests;

[TestClass]
public sealed class EfShipmentRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly ShipmentDbContext _dbContext;
    private readonly EfShipmentRepository _sut;

    public EfShipmentRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ShipmentDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ShipmentDbContext(options);
        _dbContext.Database.EnsureCreated();

        _sut = new EfShipmentRepository(_dbContext);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public async Task AddAsync_NewShipment_CanBeFoundByFindExistingAsync()
    {
        // Arrange
        var shipment = new Shipment(Guid.NewGuid(), 1, "AMBIENT-1", 10, new DateOnly(2026, 7, 13), ShipmentStatus.Booked);

        // Act
        await _sut.AddAsync(shipment);
        var found = await _sut.FindExistingAsync(1, "AMBIENT-1", new DateOnly(2026, 7, 13));

        // Assert
        Assert.IsNotNull(found);
        Assert.AreEqual(shipment.Id, found.Id);
    }

    [TestMethod]
    public async Task FindExistingAsync_NoMatchingShipment_ReturnsNull()
    {
        // Arrange

        // Act
        var found = await _sut.FindExistingAsync(99, "UNKNOWN", new DateOnly(2026, 7, 13));

        // Assert
        Assert.IsNull(found);
    }

    [TestMethod]
    public async Task AddAsync_DuplicateLocationProductAndDate_ThrowsDueToUniqueConstraint()
    {
        // Arrange
        var first = new Shipment(Guid.NewGuid(), 1, "AMBIENT-1", 10, new DateOnly(2026, 7, 13), ShipmentStatus.Booked);
        var duplicate = new Shipment(Guid.NewGuid(), 1, "AMBIENT-1", 20, new DateOnly(2026, 7, 13), ShipmentStatus.Booked);
        await _sut.AddAsync(first);

        // Act
        var addDuplicateTask = _sut.AddAsync(duplicate);

        // Assert
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => addDuplicateTask);
    }
}

// DEMO D6b: a tempting but wrong alternative.
//
// var fakeContext = Substitute.For<ShipmentDbContext>();
// var fakeShipments = Substitute.For<DbSet<Shipment>, IQueryable<Shipment>, IAsyncEnumerable<Shipment>>();
// fakeContext.Shipments.Returns(fakeShipments);
//
// This is a lie. NSubstitute can fake the *shape* of DbSet<T> and IQueryable<T>, but it
// cannot fake Entity Framework's LINQ-to-SQL translation. A query like
// `Where(s => s.DispatchDate == today)` never reaches a real query provider here, so it
// "succeeds" even for expressions that would throw against a real relational database
// (unsupported translations, client-eval differences, unique-constraint violations, and
// so on). This test would tell you your code compiles - not that it works. Use a real
// database (SQLite in-memory, as above) for anything that talks to EF Core.
