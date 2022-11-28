using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.DepartmentUpdate;
using Utg.Common.Packages.Queue;
using Utg.Common.Packages.Queue.Configuration;
using Utg.LegalService.BL.Features.Agregates.UpdateDepartment;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdateDepartmentHostedService
{
	public class UpdateDepartmentHostedService : IHostedService
	{
		private readonly ILogger<UpdateDepartmentHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;
		private readonly IMediator _mediator;

		public UpdateDepartmentHostedService(
			ILogger<UpdateDepartmentHostedService> logger,
			IQueueSubscriberService queueSubscriberService,
			RabbitMqSettings rabbitMqSettings,
			IConfiguration configuration,
			IMapper mapper, 
			IMediator mediator)
		{
			this._logger = logger;
			this._queueSubscriberService = queueSubscriberService;
			this._rabbitMqSettings = rabbitMqSettings;
			this._configuration = configuration;
			_mapper = mapper;
			_mediator = mediator;
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
			var command = _mapper.Map<UpdateUserProfileAgregateDepartmentCommand>(updateEventModel);
			await _mediator.Send(command);
		}
	}
}