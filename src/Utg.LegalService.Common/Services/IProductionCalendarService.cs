using System;
using System.Collections.Generic;

namespace Utg.LegalService.Common.Services;

public interface IProductionCalendarService
{
    bool IsBusinessDay(DateTime dateTime);
    Dictionary<DateTime, bool> AreBusinessDays(IEnumerable<DateTime> dateTimes);
}