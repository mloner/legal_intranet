using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Utg.LegalService.BL.Features.Task.NotifyExpiredSoonTasks;
using Utg.LegalService.BL.Services;
using Utg.LegalService.Common.Services;
using Utg.LegalService.Dal;
using Utg.LegalService.Dal.SqlContext;
using Xunit;

namespace Utg.LegalService.Tests;

public class AddWorkingDaysTests
{
    private readonly IProductionCalendarService _productionCalendarService;
    
    public AddWorkingDaysTests()
    {
       _productionCalendarService = new ProductionCalendarService();
    }
    
    
    [Theory]
    [InlineData("2022-11-21", 3, "2022-11-24")]
    [InlineData("2022-11-24", 3, "2022-11-29")]
    [InlineData("2022-11-24", 10, "2022-12-08")]
    [InlineData("2022-11-24", 20, "2022-12-22")]
    public async Task Test1(string dateTimeStr, int workingDaysToAdd, string lastDatetimeStr)
    {
        var dateTime = DateTime.Parse(dateTimeStr);
        var lastDatetime = DateTime.Parse(lastDatetimeStr);
        var lastDate = NotifyExpiredSoonTasksCommandHandler.AddWorkingDays(
            dateTime,
            workingDaysToAdd,
            _productionCalendarService
            );
        Assert.Equal(lastDatetime.Date, lastDate.Date);
    }
}