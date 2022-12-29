using System;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Api.Common.Models.UpdateModels.DepartmentUpdate;
using Utg.Common.Packages.Queue;
using Utg.Common.Packages.Queue.Configuration;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateDepartment;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdateDepartmentHostedService
{
	public class UpdateDepartmentHostedService : IHostedService
	{
		private readonly ILogger<UpdateDepartmentHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IServiceProvider _serviceProvider;
		
		public UpdateDepartmentHostedService(
			ILogger<UpdateDepartmentHostedService> logger,
			IQueueSubscriberService queueSubscriberService,
			RabbitMqSettings rabbitMqSettings, 
			IServiceProvider serviceProvider)
		{
			this._logger = logger;
			this._queueSubscriberService = queueSubscriberService;
			this._rabbitMqSettings = rabbitMqSettings;
			_serviceProvider = serviceProvider;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				_queueSubscriberService.SubscribeFanout<UpdateEvent<DepartmentUpdateEventModel>>(
					_rabbitMqSettings.DepartmentUpdateQueueName,
					_rabbitMqSettings.DepartmentUpdateExchangeName,
					MessageReciever);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to start {nameof(UpdateDepartmentHostedService)}");
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			this._queueSubscriberService.Unsubscribe();
			return Task.CompletedTask;
		}

		private async Task MessageReciever(UpdateEvent<DepartmentUpdateEventModel> updateEventModel)
		{
			_logger.LogInformation($"[{nameof(UpdateDepartmentHostedService)}] start");
			using (var scope = _serviceProvider.CreateScope())
			{
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
				var command = updateEventModel.Adapt<UpdateUserProfileAgregateDepartmentCommand>();
				await mediator.Send(command);
			}
			_logger.LogInformation($"[{nameof(UpdateDepartmentHostedService)}] end");
		}
	}
}