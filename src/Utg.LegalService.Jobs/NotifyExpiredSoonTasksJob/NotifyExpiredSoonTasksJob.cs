using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utg.LegalService.BL.Features.Task.NotifyExpiredSoonTasks;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.Jobs.NotifyExpiredSoonTasksJob
{
    public class NotifyExpiredSoonTasksJob : BaseJob
    {
        private readonly IMediator _mediator;
        private readonly IProductionCalendarService _productionCalendarService;
        
        public NotifyExpiredSoonTasksJob(
            ILogger<NotifyExpiredSoonTasksJob> logger,
            IConfiguration configuration, 
            IMediator mediator, 
            IProductionCalendarService productionCalendarService)
            : base(logger, configuration)
        {
            _mediator = mediator;
            _productionCalendarService = productionCalendarService;
            JobName = nameof(NotifyExpiredSoonTasksJob);
        }

        protected override async Task StartInner()
        {
            var today = DateTime.Today;
            var isBusinessDay = _productionCalendarService.IsBusinessDay(today);
            if (isBusinessDay)
            {
                var command = new NotifyExpiredSoonTasksCommand()
                {
                    DateTime = today
                };
                await _mediator.Send(command);
            }
        }
    }
}