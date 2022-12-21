using System;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.TaskChangeHistory;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.TaskChangeHistory.Create;

public class CreateTaskChangeHistoryCommandHandler 
    : IRequestHandler<CreateTaskChangeHistoryCommand, Result<TaskChangeHistoryModel>>
{
    private readonly ILogger<CreateTaskChangeHistoryCommandHandler> _logger;
    private readonly IUnitOfWork _uow;

    public CreateTaskChangeHistoryCommandHandler(
        ILogger<CreateTaskChangeHistoryCommandHandler> logger,
        IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async System.Threading.Tasks.Task<Result<TaskChangeHistoryModel>> Handle(
        CreateTaskChangeHistoryCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var history = new Common.Models.Domain.TaskChangeHistory
            {
                TaskId = command.TaskId,
                DateTime = DateTime.UtcNow,
                UserProfileId = command.UserProfileId,
                HistoryAction = command.HistoryAction,
                TaskStatus = command.TaskStatus
            };
            await _uow.TaskChangeHistoryRepository.AddAsync(history, cancellationToken);
            
            return Result<TaskChangeHistoryModel>.Created(
                history.Adapt<TaskChangeHistoryModel>());
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to create task change history.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            return Result<TaskChangeHistoryModel>.Internal(failMsg);
        }
    }

}
