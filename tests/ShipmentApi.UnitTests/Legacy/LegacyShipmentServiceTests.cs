// DEMO D1: this class has zero seams. Do not fix it.
//
// Why LegacyShipmentService cannot be unit tested:
//   - `new ShipmentDbContext()` is constructed INSIDE the method body, so no fake or
//     in-memory database can ever be substituted in - the real SQLite file on disk is
//     always touched.
//   - `new SmtpNotificationSender("smtp.corp.local")` is constructed INSIDE the method
//     body, so a test has no way to intercept the notification - it always tries to
//     reach a real (nonexistent, in this demo) SMTP host.
//   - `DateTime.UtcNow` is read directly, so the dispatch-date logic cannot be pinned to
//     a fixed instant - the test would be flaky depending on when it happens to run.
//   - `LegacyConfiguration.Instance` is a static singleton, so the cold-chain product
//     codes cannot be swapped out per test without mutating global state that leaks
//     between tests.
//
// In Jest you would write `jest.mock('./mailer')` and reach in to replace any of these
// dependencies at import time. C# has no equivalent, because the dependency is welded
// into the IL at compile time - there is no seam to intercept.
using ShipmentApi.Services.Legacy;

namespace ShipmentApi.UnitTests.Legacy;

[TestClass]
public sealed class LegacyShipmentServiceTests
{
    [TestMethod]
    [TestCategory("DemoOnly")]
    [Ignore("DEMO D1: intentionally impossible - there is no seam to inject into")]
    public void Book_ValidRequest_ReturnsBookedShipment()
    {
        // Arrange
        var sut = new LegacyShipmentService();
        var request = new BookShipmentRequest(42, "AMBIENT-1", 10);

        // Act
        var result = sut.Book(request);

        // Assert
        Assert.AreEqual(ShipmentStatus.Booked, result.Status);
    }
}
