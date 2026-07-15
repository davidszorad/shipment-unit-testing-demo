namespace ShipmentApi.UnitTests;

// DEMO D3: no mocks needed. Write these first, always.
[TestClass]
public sealed class ColdChainRulesTests
{
    [TestMethod]
    [DataRow(StorageClass.Ambient, false)]
    [DataRow(StorageClass.Chilled, true)]
    [DataRow(StorageClass.Frozen, true)]
    public void RequiresColdChain_AllStorageClasses_ReturnsExpectedResult(StorageClass storageClass, bool expected)
    {
        // Arrange
        var product = new Product("SKU-1", storageClass);

        // Act
        var result = ColdChainRules.RequiresColdChain(product);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Calculate_BeforeCutOff_DispatchesSameDay()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 7, 13, 9, 0, 0, TimeSpan.Zero); // Monday 09:00 UTC

        // Act
        var dispatchDate = DispatchWindow.Calculate(now);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 13), dispatchDate);
    }

    [TestMethod]
    public void Calculate_ExactlyAtCutOff_DispatchesNextDay()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 7, 13, 16, 0, 0, TimeSpan.Zero); // Monday 16:00 UTC

        // Act
        var dispatchDate = DispatchWindow.Calculate(now);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 14), dispatchDate);
    }

    [TestMethod]
    public void Calculate_AfterCutOff_DispatchesNextDay()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 7, 13, 16, 30, 0, TimeSpan.Zero); // Monday 16:30 UTC

        // Act
        var dispatchDate = DispatchWindow.Calculate(now);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 14), dispatchDate);
    }

    [TestMethod]
    public void Calculate_FridayAfterCutOff_RollsToFollowingMonday()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 7, 17, 16, 30, 0, TimeSpan.Zero); // Friday 16:30 UTC

        // Act
        var dispatchDate = DispatchWindow.Calculate(now);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 20), dispatchDate); // following Monday
    }

    [TestMethod]
    public void Calculate_Saturday_RollsToMonday()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 7, 18, 9, 0, 0, TimeSpan.Zero); // Saturday 09:00 UTC

        // Act
        var dispatchDate = DispatchWindow.Calculate(now);

        // Assert
        Assert.AreEqual(new DateOnly(2026, 7, 20), dispatchDate); // Monday
    }
}
