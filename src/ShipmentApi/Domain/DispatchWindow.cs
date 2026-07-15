namespace ShipmentApi.Domain;

/// <summary>
/// Pure business logic, zero dependencies. This is why D3 needs no mocks.
/// </summary>
public static class DispatchWindow
{
    private static readonly TimeOnly CutOffTime = new(16, 0);

    /// <summary>
    /// Determines the dispatch date for a booking made at <paramref name="now"/> (UTC).
    /// At or after 16:00 UTC rolls to the next day; Saturday/Sunday roll to Monday.
    /// </summary>
    public static DateOnly Calculate(DateTimeOffset now)
    {
        var utcNow = now.ToUniversalTime();
        var candidateDate = DateOnly.FromDateTime(utcNow.DateTime);
        var candidateTime = TimeOnly.FromDateTime(utcNow.DateTime);

        if (candidateTime >= CutOffTime)
        {
            candidateDate = candidateDate.AddDays(1);
        }

        return RollPastWeekend(candidateDate);
    }

    private static DateOnly RollPastWeekend(DateOnly date) => date.DayOfWeek switch
    {
        DayOfWeek.Saturday => date.AddDays(2),
        DayOfWeek.Sunday => date.AddDays(1),
        _ => date,
    };
}
