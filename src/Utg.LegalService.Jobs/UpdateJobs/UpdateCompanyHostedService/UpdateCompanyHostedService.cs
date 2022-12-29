using System;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Api.Common.Models.UpdateModels.CompanyUpdate;
using Utg.Common.Packages.Queue;
using Utg.Common.Packages.Queue.Configuration;
using Utg.LegalService.BL.Features.UserProfileAggregates.UpdateCompany;

namespace Utg.LegalService.Jobs.UpdateJobs.UpdateCompanyHostedService
{
	public class UpdateCompanyHostedService : IHostedService
	{
		private readonly ILogger<UpdateCompanyHostedService> _logger;
		private readonly IQueueSubscriberService _queueSubscriberService;
		private readonly RabbitMqSettings _rabbitMqSettings;
		private readonly IServiceProvider _serviceProvider;

		public UpdateCompanyHostedService(
			ILogger<UpdateCompanyHostedService> logger,
			IQueueSubscriberService queueSubscriberService,
			RabbitMqSettings rabbitMqSettings,
			IMediator mediator, 
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
			_logger.LogInformation($"[{nameof(UpdateCompanyHostedService)}] start");
			using (var scope = _serviceProvider.CreateScope())
			{
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
				var command = updateEventModel.Adapt<UpdateUserProfileAgregateCompanyCommand>();
				await mediator.Send(command);
			}
			_logger.LogInformation($"[{nameof(UpdateCompanyHostedService)}] end");
		}
	}
}
