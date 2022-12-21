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
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateUserProfile;
using Utg.LegalService.Common.Models.UpdateModels;
using Utg.LegalService.Common.Models.UpdateModels.UserProfileUpdate;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdateUserProfileHostedService
{
	public class UpdateUserProfileHostedService : IHostedService
	{
		private readonly ILogger<UpdateUserProfileHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IServiceProvider _serviceProvider;

		public UpdateUserProfileHostedService(
			ILogger<UpdateUserProfileHostedService> logger,
			IQueueSubscriberService queueSubscriberService,
			RabbitMqSettings rabbitMqSettings, 
			IServiceProvider serviceProvider)
		{
			_logger = logger;
			_queueSubscriberService = queueSubscriberService;
			_rabbitMqSettings = rabbitMqSettings;
			_serviceProvider = serviceProvider;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				_queueSubscriberService.SubscribeFanout<UpdateEvent<UserProfileUpdateEventModel>>(
					_rabbitMqSettings.HireQueueName,
					_rabbitMqSettings.HireExchangeName,
					MessageReciever);
				_queueSubscriberService.SubscribeFanout<UpdateEvent<UserProfileUpdateEventModel>>(
					_rabbitMqSettings.QuitQueueName,
					_rabbitMqSettings.QuitExchangeName,
					MessageReciever);
				_queueSubscriberService.SubscribeFanout<UpdateEvent<UserProfileUpdateEventModel>>(
					_rabbitMqSettings.UserProfileUpdateQueueName,
					_rabbitMqSettings.UserProfileUpdateExchangeName,
					MessageReciever);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to start {nameof(UpdateUserProfileHostedService)}");
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_queueSubscriberService.Unsubscribe();
			return Task.CompletedTask;
		}

		private async Task MessageReciever(UpdateEvent<UserProfileUpdateEventModel> updateEventModel)
		{
			_logger.LogInformation($"[{nameof(UpdateUserProfileHostedService)}] start");
			using (var scope = _serviceProvider.CreateScope())
			{
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
				var command = updateEventModel.Adapt<UpdateUserProfileAgregateCommand>();
				await mediator.Send(command);
			}
			_logger.LogInformation($"[{nameof(UpdateUserProfileHostedService)}] end");
		}
	}
}
