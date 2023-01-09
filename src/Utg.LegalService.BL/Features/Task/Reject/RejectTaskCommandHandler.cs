using System;
using System.Linq;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.BL.Features.Attachments.Create;
using Utg.LegalService.BL.Features.Attachments.Delete;
using Utg.LegalService.BL.Features.SubTask.CreateEmitEvents;
using Utg.LegalService.BL.Features.Task.Get;
using Utg.LegalService.BL.Features.TaskChangeHistory.Create;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Task.Reject;

public class RejectTaskCommandHandler 
    : IRequestHandler<RejectTaskCommand, Result<TaskModel>>
{
    private readonly ILogger<RejectTaskCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _uow;

    public RejectTaskCommandHandler(
        ILogger<RejectTaskCommandHandler> logger,
        IUnitOfWork uow, 
        IMediator mediator)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task<Result<TaskModel>> Handle(
        RejectTaskCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _uow.TaskRepository
                .GetQuery(x => x.Id == command.Id, null)
                .FirstOrDefaultAsync(cancellationToken);
            if (task == null)
            {
                return Result<TaskModel>.NotFound("Task not found");
            }

            //change status
            task.Status = TaskStatus.Rejected;

            //add comment
            var comment = new TaskComment()
            {
                TaskId = task.Id,
                Text = $"Причина отклонения: {command.Reason}",
                DateTime = DateTime.UtcNow,
                UserProfileId = command.AuthInfo.UserProfileId
            };
            await _uow.TaskCommentRepository.AddAsync(comment, cancellationToken);
            
            //add history
            var history = new Common.Models.Domain.TaskChangeHistory
            {
                TaskId = task.Id,
                DateTime = DateTime.UtcNow,
                UserProfileId = command.AuthInfo.UserProfileId,
                HistoryAction = HistoryAction.StatusChanged,
                TaskStatus = TaskStatus.Rejected
            };
            await _uow.TaskChangeHistoryRepository.AddAsync(history, cancellationToken);

            //reject subtasks
            var subtasks = await _uow.TaskRepository
                .GetQuery(x => x.ParentTaskId == task.Id, null)
                .ToListAsync(cancellationToken);
            var subtaskTasks = subtasks.Select(async subtask =>
            {
                var rejectSubtaskTaskCommand = new RejectTaskCommand()
                {
                    Id = subtask.Id,
                    Reason = command.Reason,
                    AuthInfo = command.AuthInfo
                };
                await _mediator.Send(rejectSubtaskTaskCommand, cancellationToken);
            });
            await System.Threading.Tasks.Task.WhenAll(subtaskTasks);
            
            await _uow.SaveChangesAsync(cancellationToken);
            
            //get task
            var getTaskCom = new GetTaskCommand()
            {
                Id = task.Id,
                AuthInfo = command.AuthInfo
            };
            var getTaskComResp = await _mediator.Send(getTaskCom, cancellationToken);
            if (!getTaskComResp.Success)
            {
                return Result<TaskModel>.Failed(getTaskComResp);
            }
            var resultTask = getTaskComResp.Data;

            
            return Result<TaskModel>.Ok(resultTask);
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to reject task.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result<TaskModel>.Internal(failMsg);
        }
    }

}
