# Live Refactor: D7 (Brittle vs. Behaviour)

Goal: change **zero observable behaviour** of `ShipmentService.BookAsync`, but make
`BookAsync_ValidRequest_BrittleImplementationCoupledTest` fail while
`BookAsync_ValidRequest_BehaviourTest` (and every other test) stays green.

The refactor extracts the persistence + notification steps into a private method and
swaps their order (notify, then persist, instead of persist, then notify). The returned
`Shipment`, its `Status`/`DispatchDate`, and the fact that exactly one confirmation is
sent are all unchanged - only the *order* of two side-effecting calls changes, which is
exactly what the brittle test's `Received.InOrder(...)` locks in.

## Before

File: [src/ShipmentApi/Services/ShipmentService.cs](../src/ShipmentApi/Services/ShipmentService.cs)

```csharp
        var shipment = new Shipment(
            Guid.NewGuid(),
            request.LocationId,
            request.ProductCode,
            request.Quantity,
            dispatchDate,
            ShipmentStatus.Booked);

        await shipmentRepository.AddAsync(shipment, cancellationToken);

        var confirmation = new BookingConfirmation(
            shipment.LocationId,
            shipment.ProductCode,
            shipment.Quantity,
            shipment.DispatchDate,
            shipment.Id);

        await notificationSender.SendBookingConfirmationAsync(confirmation, cancellationToken);

        logger.LogInformation(
            "Booked shipment {ShipmentId} for location {LocationId} dispatching {DispatchDate}",
            shipment.Id,
            shipment.LocationId,
            shipment.DispatchDate);

        return shipment;
    }
}
```

## After

```csharp
        var shipment = new Shipment(
            Guid.NewGuid(),
            request.LocationId,
            request.ProductCode,
            request.Quantity,
            dispatchDate,
            ShipmentStatus.Booked);

        await PersistAndNotifyAsync(shipment, cancellationToken);

        return shipment;
    }

    private async Task PersistAndNotifyAsync(Shipment shipment, CancellationToken cancellationToken)
    {
        var confirmation = new BookingConfirmation(
            shipment.LocationId,
            shipment.ProductCode,
            shipment.Quantity,
            shipment.DispatchDate,
            shipment.Id);

        await notificationSender.SendBookingConfirmationAsync(confirmation, cancellationToken);
        await shipmentRepository.AddAsync(shipment, cancellationToken);

        logger.LogInformation(
            "Booked shipment {ShipmentId} for location {LocationId} dispatching {DispatchDate}",
            shipment.Id,
            shipment.LocationId,
            shipment.DispatchDate);
    }
}
```

## What to say while you apply it

1. Paste the "After" block over the "Before" block in `ShipmentService.cs` (both start at
   `var shipment = new Shipment(` and end at the closing `}` of the class - a single
   selection-and-replace, under 20 seconds).
2. Run `dotnet test --filter "FullyQualifiedName~BrittleVsBehaviourTests"`.
3. Point at the red `BookAsync_ValidRequest_BrittleImplementationCoupledTest` and the
   `CallSequenceNotFoundException` NSubstitute prints - it shows the *expected* order
   next to the *actual* order.
4. Point at the still-green `BookAsync_ValidRequest_BehaviourTest`: same request, same
   response, same one-confirmation guarantee - nothing about what the caller can observe
   changed.
5. Revert (undo, or paste the "Before" block back) before moving on.

## Why this is the point of D7

The brittle test encodes an implementation detail (call order between two independent
side effects) as if it were a requirement. Nothing in the business rules says "persist
before notifying" - that ordering is an accident of how `BookAsync` happens to be
written today. A refactor that preserves every business rule still breaks that test,
which means the test was coupled to *how* the SUT works, not *what* it guarantees. Write
tests like `BookAsync_ValidRequest_BehaviourTest` instead: assert on inputs and outputs,
not on the sequence of internal collaborator calls.
