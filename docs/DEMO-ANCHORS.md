# Demo Anchors

Quick-reference map from the talk's demo beats (D1-D7) to exact files, lines, and the
one-sentence point to make while showing them. Line numbers refer to the files as
committed; re-check them if the files are edited before the talk.

| # | File | What to show | Talking point |
|---|------|---------------|----------------|
| D1 | [src/ShipmentApi/Services/Legacy/LegacyShipmentService.cs](../src/ShipmentApi/Services/Legacy/LegacyShipmentService.cs) and [tests/ShipmentApi.UnitTests/Legacy/LegacyShipmentServiceTests.cs](../tests/ShipmentApi.UnitTests/Legacy/LegacyShipmentServiceTests.cs) | The `new ShipmentDbContext()`, `new SmtpNotificationSender(...)`, `DateTime.UtcNow`, and `LegacyConfiguration.Instance` lines inside `Book(...)`; then the comment block above `LegacyShipmentServiceTests` | "In Jest, `jest.mock('./mailer')` rewrites the module graph at import time; in C# the dependency is compiled straight into the IL, so there is no seam - untestable code isn't a testing problem, it's a design problem." |
| D3 | [tests/ShipmentApi.UnitTests/ColdChainRulesTests.cs](../tests/ShipmentApi.UnitTests/ColdChainRulesTests.cs) | The `[DataRow]` triple for `RequiresColdChain`, and the five `DispatchWindow.Calculate` tests | "Zero substitutes, zero setup - pure functions are the cheapest tests you will ever write, so isolate business rules into them whenever you can." |
| D4 | [tests/ShipmentApi.UnitTests/ShipmentServiceTests.cs](../tests/ShipmentApi.UnitTests/ShipmentServiceTests.cs) | The constructor building four `Substitute.For<T>()` fields; `BookAsync_ValidRequest_SendsExactlyOneConfirmation` (`Received(1)` + `Arg.Is`); `BookAsync_ColdChainProductAtWarmLocation_DoesNotSendNotification` (`DidNotReceive`); `BookAsync_ValidRequest_CapturesShipmentPassedToRepository` (`Arg.Do`) | "This is the NSubstitute vocabulary you'll use in 90% of real tests: `Arg.Any`, `Arg.Is`, `.Returns`, `Received(n)`, `DidNotReceive` - all English verbs, no framework ceremony." |
| D5 | [tests/ShipmentApi.UnitTests/ShipmentServiceValidationTests.cs](../tests/ShipmentApi.UnitTests/ShipmentServiceValidationTests.cs) | The `[DataRow(0)]`/`[DataRow(-1)]`/`[DataRow(501)]` block and the `[DynamicData(nameof(ColdChainMatrix))]` matrix test | "`DataRow` is `test.each` for simple literals; `DynamicData` is `test.each` for a computed table - same idea as Jest, different attribute." |
| D6a | [tests/ShipmentApi.UnitTests/TimeAndLoggingTests.cs](../tests/ShipmentApi.UnitTests/TimeAndLoggingTests.cs) | The three `FakeTimeProvider` tests, then `BookAsync_ValidRequest_LogsOneInformationRecord` and the comment immediately below it | "`FakeTimeProvider` kills flaky date-based tests entirely - but the logging test is a warning, not a template: logs are an implementation detail, not behaviour." |
| D6b | [tests/ShipmentApi.IntegrationTests/EfShipmentRepositoryTests.cs](../tests/ShipmentApi.IntegrationTests/EfShipmentRepositoryTests.cs) | The constructor (real `SqliteConnection` + `EnsureCreated`), `AddAsync_DuplicateLocationProductAndDate_ThrowsDueToUniqueConstraint`, and the commented-out `Substitute.For<ShipmentDbContext>()` block at the bottom | "Mocking your own database context tests that your code compiles against a fake API surface - it can never tell you whether your LINQ actually translates to SQL. For EF, use a real (in-memory) database." |
| D7 | [tests/ShipmentApi.UnitTests/BrittleVsBehaviourTests.cs](../tests/ShipmentApi.UnitTests/BrittleVsBehaviourTests.cs) | Both test bodies side by side, then live-apply [docs/REFACTOR.md](REFACTOR.md) | "Same feature, same outcome, two tests - one breaks on a harmless refactor, one doesn't. Assert on what the caller can observe, never on how the SUT gets there." |

## Suggested run order during the talk

1. `dotnet test` (whole solution, filtered) to show green baseline - see [README.md](../README.md).
2. D3 → D4 → D5 → D6a → D6b in file order above (increasing sophistication).
3. D1 last among the "how to test" material, to land the "this is why seams matter" point.
4. D7 as the closing live-edit: apply [docs/REFACTOR.md](REFACTOR.md), re-run, watch the
   brittle test go red while the behaviour test and everything else stays green, then
   revert.
