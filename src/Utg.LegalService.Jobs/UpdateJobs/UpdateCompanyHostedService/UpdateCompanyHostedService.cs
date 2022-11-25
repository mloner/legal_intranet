using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.CompanyUpdate;
using Utg.Common.Packages.Queue;
using Utg.Common.Packages.Queue.Configuration;
using Utg.LegalService.BL.Features.Agregates.UpdateCompany;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdateCompanyHostedService
{
	public class UpdateCompanyHostedService : IHostedService
	{
		private readonly ILogger<UpdateCompanyHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;
		private readonly IMediator _mediator;

		public UpdateCompanyHostedService(
			ILogger<UpdateCompanyHostedService> logger,
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
				_queueSubscriberService.SubscribeFanout<UpdateEvent<CompanyUpdateEventModel>>(
					_rabbitMqSettings.CompanyUpdateQueueName,
					_rabbitMqSettings.CompanyUpdateExchangeName,
					MessageReciever);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to start {nameof(UpdateCompanyHostedService)}");
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_queueSubscriberService.Unsubscribe();
			return Task.CompletedTask;
		}

		private async Task MessageReciever(UpdateEvent<CompanyUpdateEventModel> updateEventModel)
		{
			var command = _mapper.Map<UpdateUserProfileAgregateCompanyCommand>(updateEventModel);
			await _mediator.Send(command);
		}
	}
}
