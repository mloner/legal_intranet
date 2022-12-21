using System;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Queue;
using Utg.Common.Packages.Queue.Configuration;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdatePosition;
using Utg.LegalService.Common.Models.UpdateModels;
using Utg.LegalService.Common.Models.UpdateModels.PositionUpdate;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdatePositionHostedService
{
	public class UpdatePositionHostedService : IHostedService
	{
		private readonly ILogger<UpdatePositionHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IServiceProvider _serviceProvider;

		public UpdatePositionHostedService(
			ILogger<UpdatePositionHostedService> logger,
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
				_queueSubscriberService.SubscribeFanout<UpdateEvent<PositionUpdateEventModel>>(
					_rabbitMqSettings.PositionUpdateQueueName,
					_rabbitMqSettings.PositionUpdateExchangeName,
					MessageReceiver);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to start {nameof(UpdatePositionHostedService)}");
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			this._queueSubscriberService.Unsubscribe();
			return Task.CompletedTask;
		}

		private async Task MessageReceiver(UpdateEvent<PositionUpdateEventModel> updateEventModel)
		{
			_logger.LogInformation($"[{nameof(UpdatePositionHostedService)}] start");
			using (var scope = _serviceProvider.CreateScope())
			{
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
				var command = updateEventModel.Adapt<UpdateUserProfileAgregatePositionCommand>();
				await mediator.Send(command);
			}
			_logger.LogInformation($"[{nameof(UpdatePositionHostedService)}] end");
		}
	}
}
