namespace ShipmentApi.UnitTests;

/// <summary>
/// DEMO: the full MSTest lifecycle, broadest scope to narrowest:
///
///   AssemblyInitialize                                  (once, before any test in the assembly)
///     ClassInitialize                                   (once, before any test in this class)
///       TestInitialize -> [TestMethod] -> TestCleanup    (once per test)
///     ClassCleanup                                       (once, after every test in this class)
///   AssemblyCleanup                                      (once, after every test in the assembly)
///
/// There is no Jest equivalent to the assembly/class scoped hooks - the closest analogy is
/// a global Jest setup file (globalSetup/globalTeardown) for AssemblyInitialize/
/// AssemblyCleanup, and a `describe` block's `beforeAll`/`afterAll` for ClassInitialize/
/// ClassCleanup. Run this class with `--verbosity detailed` (or check the test output pane)
/// to see the console/TestContext lines print in the order above.
///
/// Only ONE [AssemblyInitialize] method and ONE [AssemblyCleanup] method are allowed per
/// test assembly - both are defined here since this is the only class in the assembly that
/// demonstrates assembly-level hooks.
/// </summary>
[TestClass]
public sealed class LifecycleDemoTests
{
    private static int s_classInitializeCount;

    public TestContext TestContext { get; set; } = null!;

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        context.WriteLine("AssemblyInitialize: runs once, before any test in this assembly.");
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        // No TestContext parameter is available here - AssemblyCleanup runs after the whole
        // assembly is done, so Console.WriteLine (captured by the test host) is used instead.
        Console.WriteLine("AssemblyCleanup: runs once, after every test in this assembly.");
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        s_classInitializeCount++;
        context.WriteLine($"ClassInitialize: runs once for {nameof(LifecycleDemoTests)} (call #{s_classInitializeCount}).");
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Console.WriteLine($"ClassCleanup: runs once for {nameof(LifecycleDemoTests)}.");
    }

    [TestInitialize]
    public void TestInitialize()
    {
        TestContext.WriteLine($"TestInitialize: before {TestContext.TestName}.");
    }

    [TestCleanup]
    public void TestCleanup()
    {
        TestContext.WriteLine($"TestCleanup: after {TestContext.TestName}.");
    }

    [TestMethod]
    public void LifecycleOrder_FirstTestInClass_ClassInitializeAlreadyRanExactlyOnce()
    {
        // Arrange

        // Act

        // Assert
        Assert.AreEqual(1, s_classInitializeCount);
    }

    [TestMethod]
    public void LifecycleOrder_SecondTestInClass_ClassInitializeStillRanExactlyOnce()
    {
        // Arrange

        // Act

        // Assert
        Assert.AreEqual(1, s_classInitializeCount);
    }
}
