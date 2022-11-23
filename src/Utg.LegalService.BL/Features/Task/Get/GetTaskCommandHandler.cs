using System;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Task.Get;

public class GetTaskCommandHandler 
    : IRequestHandler<GetTaskCommand, Result<TaskModel>>
{
    private readonly ILogger<GetTaskCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly UnitOfWork _uow;

    public GetTaskCommandHandler(
        ILogger<GetTaskCommandHandler> logger,
        UnitOfWork uow, 
        IMediator mediator)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task<Result<TaskModel>> Handle(
        GetTaskCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _uow.TaskItems.GetQuery(
                    x => x.Id == command.Id,
                    null)
                .Include(x => x.TaskAttachments)
                .FirstOrDefaultAsync(cancellationToken);
            if (task == null)
            {
                return Result<TaskModel>.NotFound("No such task");
            }
            
            
            
            return Result<TaskModel>.Ok(task.Adapt<TaskModel>());
        }
        catch (Exception e)
        {
            var failMsg = "Failed to get task.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result<TaskModel>.Internal(failMsg);
        }
    }

}
