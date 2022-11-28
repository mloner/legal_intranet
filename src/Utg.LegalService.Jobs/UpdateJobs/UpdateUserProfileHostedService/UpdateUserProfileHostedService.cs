using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.UserProfileUpdate;
using Utg.Common.Packages.Queue;
using Utg.Common.Packages.Queue.Configuration;
using Utg.LegalService.BL.Features.Agregates.UpdateUserProfile;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdateUserProfileHostedService
{
	public class UpdateUserProfileHostedService : IHostedService
	{
		private readonly ILogger<UpdateUserProfileHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;
		private readonly IMediator _mediator;

		public UpdateUserProfileHostedService(
			ILogger<UpdateUserProfileHostedService> logger,
			IQueueSubscriberService queueSubscriberService,
			RabbitMqSettings rabbitMqSettings,
			IConfiguration configuration, 
			IMapper mapper,
			IMediator mediator)
		{
			_logger = logger;
			_queueSubscriberService = queueSubscriberService;
			_rabbitMqSettings = rabbitMqSettings;
			_configuration = configuration;
			_mapper = mapper;
			_mediator = mediator;
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
			var command = _mapper.Map<UpdateUserProfileAgregateCommand>(updateEventModel);
			await _mediator.Send(command);
		}
	}
}
