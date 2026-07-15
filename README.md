# UnitTestingDemo

Live-demo companion repository for the talk **"Unit Testing in C#: MSTest + NSubstitute
for developers coming from Jest/Next.js."** It is a small shipment-booking API
(`ShipmentApi`) with two service implementations of the same business rules - one built
for testability (`ShipmentService`) and one deliberately built without any seams
(`LegacyShipmentService`) - plus a full test suite that demonstrates pure-logic tests,
NSubstitute mocking, fake time/logging, integration tests against a real database, and
the difference between brittle and behavioural assertions. See
[docs/DEMO-ANCHORS.md](docs/DEMO-ANCHORS.md) for the file-by-file talk script and
[docs/REFACTOR.md](docs/REFACTOR.md) for the live D7 refactor.

## Prerequisites

- .NET SDK 10.0 or later ([dotnet.microsoft.com/download](https://dotnet.microsoft.com/download))
- No database server, Docker, or other external service required - integration tests use
  SQLite in-memory.

## Running the tests

Restore and build the whole solution:

```bash
dotnet restore
dotnet build
```

Run the default (CI-safe) test suite - this excludes the intentionally-impossible D1
artefact:

```bash
dotnet test --settings unittestingdemo.runsettings
```

Equivalently, without the settings file, using a filter expression:

```bash
dotnet test --filter "TestCategory!=DemoOnly"
```

Run a single project:

```bash
dotnet test tests/ShipmentApi.UnitTests/ShipmentApi.UnitTests.csproj --settings unittestingdemo.runsettings
dotnet test tests/ShipmentApi.IntegrationTests/ShipmentApi.IntegrationTests.csproj
```

## Running only the `DemoOnly` category

The `DemoOnly` category currently contains exactly one test:
`LegacyShipmentServiceTests.Book_ValidRequest_ReturnsBookedShipment`, which is marked
`[Ignore(...)]` because it cannot be made to pass without a live SMTP server and a real
clock (see [D1 in docs/DEMO-ANCHORS.md](docs/DEMO-ANCHORS.md)). To see it listed
(skipped, not run) - scoped to the unit test project, since the integration test project
has no `DemoOnly` tests and would otherwise report "zero tests ran" as an error:

```bash
dotnet test tests/ShipmentApi.UnitTests/ShipmentApi.UnitTests.csproj --filter "TestCategory=DemoOnly"
```

To run the full suite including this category (it will report as skipped, not failed):

```bash
dotnet test
```

## Running the API

```bash
dotnet run --project src/ShipmentApi
```

Then check the health endpoint:

```bash
curl http://localhost:5000/health
```

(Port may vary - check the console output for the actual URLs `dotnet run` binds to.)

## Solution layout

```
src/ShipmentApi/                    Minimal API: domain, services, EF Core infrastructure
tests/ShipmentApi.UnitTests/        MSTest v4 + NSubstitute unit tests (MSTest.Sdk)
tests/ShipmentApi.IntegrationTests/ MSTest v4 tests against real SQLite + WebApplicationFactory
docs/DEMO-ANCHORS.md                D1-D7 talk script: file, lines, talking point
docs/REFACTOR.md                    Copy-pasteable live refactor for the D7 demo beat
```
