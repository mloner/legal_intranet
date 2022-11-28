using System;
using System.Collections.Generic;
using System.Linq;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.BL.Services;

public class ProductionCalendarService : IProductionCalendarService
{
    public bool IsBusinessDay(DateTime dateTime)
    {
        return dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday;
    }

    public Dictionary<DateTime, bool> AreBusinessDays(IEnumerable<DateTime> dateTimes)
    {
        return dateTimes.ToDictionary(
            x => x,
            IsBusinessDay);
    }
}