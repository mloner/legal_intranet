using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.PositionUpdate;
using Utg.Common.Packages.Queue;
using Utg.Common.Packages.Queue.Configuration;
using Utg.LegalService.BL.Features.Agregates.UpdatePosition;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdatePositionHostedService
{
	public class UpdatePositionHostedService : IHostedService
	{
		private readonly ILogger<UpdatePositionHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IConfiguration _configuration;
		private readonly IMediator _mediator;
		private readonly IMapper _mapper;

		public UpdatePositionHostedService(
			ILogger<UpdatePositionHostedService> logger,
			IQueueSubscriberService queueSubscriberService,
			RabbitMqSettings rabbitMqSettings,
			IConfiguration configuration, 
			IMediator mediator, 
			IMapper mapper)
		{
			this._logger = logger;
			this._queueSubscriberService = queueSubscriberService;
			this._rabbitMqSettings = rabbitMqSettings;
			this._configuration = configuration;
			_mediator = mediator;
			_mapper = mapper;
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
			var command = _mapper.Map<UpdateUserProfileAgregatePositionCommand>(updateEventModel);
			await _mediator.Send(command);
		}
	}
}
