﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.LegalService.BL.Features.AccessRights.Get;
using Utg.LegalService.BL.Features.Attachments.Create;
using Utg.LegalService.BL.Features.Attachments.Delete;
using Utg.LegalService.BL.Features.SubTask.CreateEmitEvents;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Domain;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.SubTask.Create;

public class CreateSubtaskCommandHandler 
    : IRequestHandler<CreateSubtaskCommand, Result<SubtaskModel>>
{
    private readonly ILogger<CreateSubtaskCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly UnitOfWork _uow;

    public CreateSubtaskCommandHandler(
        ILogger<CreateSubtaskCommandHandler> logger,
        UnitOfWork uow, 
        IMediator mediator)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task<Result<SubtaskModel>> Handle(
        CreateSubtaskCommand command, 
        CancellationToken cancellationToken)
    {
        var attachments = Enumerable.Empty<TaskAttachmentModel>();
        try
        {
            var parentTask = await _uow.TaskItems.GetQuery(
                x => x.Id == command.ParentTaskId,
                null
                ).FirstOrDefaultAsync(cancellationToken);
            if (parentTask == null)
            {
                return Result<SubtaskModel>.Bad("No such parent task");
            }
            var now = DateTime.UtcNow;
            var task = new Common.Models.Domain.Task
            {
                Status = command.Status,
                Type = command.Type,
                Description = command.Description,
                AuthorUserProfileId = command.AuthInfo.UserProfileId,
                AuthorFullName = command.AuthInfo.FullName,
                PerformerUserProfileId = parentTask.PerformerUserProfileId,
                PerformerFullName = parentTask.PerformerFullName,
                CreationDateTime = now,
                LastChangeDateTime = now,
                DeadlineDateTime = command.DeadLine,
                ParentTaskId = command.ParentTaskId
            };

            await _uow.TaskItems.AddAsync(task, cancellationToken);
            
            var createAttachmentsComResp = 
                await _mediator.Send(new CreateAttachmentsCommand()
                {
                    TaskId = task.Id,
                    Attachments = command.Attachments,
                    AuthInfo = command.AuthInfo
                }, cancellationToken);
            if (!createAttachmentsComResp.Success)
            {
                return Result<SubtaskModel>.Failed(createAttachmentsComResp);
            }
            attachments = createAttachmentsComResp.Data;
            
            var emitEventsResp = 
                await _mediator.Send(new CreateSubtaskEmitEventsCommand()
                {
                    Task = task.Adapt<SubtaskModel>(),
                    AuthInfo = command.AuthInfo
                }, cancellationToken);
            if (!emitEventsResp.Success)
            {
                return Result<SubtaskModel>.Failed(emitEventsResp);
            }

            var resultTask = await _uow.TaskItems.GetQuery(
                x => x.Id == task.Id,
                null).
                Include(x => x.TaskAttachments)
                .FirstOrDefaultAsync(cancellationToken);
            var taskModel = resultTask.Adapt<SubtaskModel>();
            
            var getTarCommand = new GetTaskAccessRightsCommand()
            {
                Task = taskModel,
                AuthInfo = command.AuthInfo
            };
            var getTarsCommandResp = 
                await _mediator.Send(getTarCommand, cancellationToken);
            if(!getTarsCommandResp.Success)
                return Result<SubtaskModel>.Failed(getTarsCommandResp);
            taskModel.AccessRights = getTarsCommandResp.Data;
            
            return Result<SubtaskModel>.Created(
                resultTask.Adapt<SubtaskModel>());
        }
        catch (Exception e)
        {
            var deleteAttachmentFilesResp = 
                await _mediator.Send(new DeleteAttachmentFilesCommand()
                {
                    Attachments = attachments
                }, cancellationToken);
            if (!deleteAttachmentFilesResp.Success)
            {
                return Result<SubtaskModel>.Failed(deleteAttachmentFilesResp);
            }
            _logger.LogError(e, "Failed to add subtask. {@Command}", command);
            
            return Result<SubtaskModel>.Internal("Failed to add subtask.");
        }
    }

}
