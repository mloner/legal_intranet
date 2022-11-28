using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.BL.Features.AccessRights.Get;
using Utg.LegalService.BL.Features.Agregates.GetList;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Attachment;
using Utg.LegalService.Common.Models.Client.Comment;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Task.Get;

public class GetTaskCommandHandler 
    : IRequestHandler<GetTaskCommand, Result<TaskModel>>
{
    private readonly ILogger<GetTaskCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly UnitOfWork _uow;
    private readonly IUsersProxyClient _usersProxyClient;

    public GetTaskCommandHandler(
        ILogger<GetTaskCommandHandler> logger,
        UnitOfWork uow, 
        IMediator mediator,
        IUsersProxyClient usersProxyClient)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
        _usersProxyClient = usersProxyClient;
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
            
            var taskModel = task.Adapt<TaskModel>();
            
            var getTaskAccRightsComResp = 
                await _mediator.Send(new GetTaskAccessRightsCommand()
                {
                    Task = taskModel,
                    AuthInfo = command.AuthInfo
                }, cancellationToken);
            if (!getTaskAccRightsComResp.Success)
            {
                return Result<TaskModel>.Failed(getTaskAccRightsComResp);
            }
            taskModel.AccessRights = getTaskAccRightsComResp.Data;
            
            taskModel.TaskComments = await GetTaskComments(command, cancellationToken);

            taskModel.Attachments = await FillAttachmentRights(taskModel, command.AuthInfo);

            return Result<TaskModel>.Ok(taskModel);
        }
        catch (Exception e)
        {
            var failMsg = "Failed to get task.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);
            
            return Result<TaskModel>.Internal(failMsg);
        }
    }

    private async System.Threading.Tasks.Task<IEnumerable<TaskCommentModel>> GetTaskComments(
        GetTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var comments = await _uow.TaskCommentItems
            .GetQuery(x => x.TaskId == command.Id,
                null)
            .ToListAsync(cancellationToken);
            
        var userProfileIds = comments.Select(x => x.UserProfileId);
        var getAgregatesCommand = new GetListUserProfileAgregatesCommand()
        {
            UserProfileIds = userProfileIds
        };
        var getAgregatesCommandResponse = await _mediator.Send(getAgregatesCommand, cancellationToken);
        var userProfiles = getAgregatesCommandResponse.Data;
        var models = comments.Select(x => x.Adapt<TaskCommentModel>());
        
        foreach (var model in models)
        {
            var userProfile = 
                userProfiles.FirstOrDefault(x => x.UserProfileId == model.UserProfileId);
            if (userProfile != null)
            {
                model.UserProfileFullName = userProfile.FullName;
            }
        }

        return models;
    }

    private async System.Threading.Tasks.Task<IEnumerable<TaskAttachmentModel>>
        FillAttachmentRights(TaskModel taskModel,
            AuthInfo authInfo)
    {
        var attachmentsTasks = taskModel.Attachments.Select(async attachment => 
        {
            attachment.AccessRights = await GetAttachmentAccessRights(
                taskModel,
                attachment,
                authInfo);
            return attachment;
        }).ToList();
        taskModel.Attachments = await System.Threading.Tasks.Task.WhenAll(attachmentsTasks);
        return taskModel.Attachments;
    }

    private async System.Threading.Tasks.Task<AttachmentAccessRights> GetAttachmentAccessRights(TaskModel task,
        TaskAttachmentModel attachment, AuthInfo authInfo)
    {
        var result = new AttachmentAccessRights();
        if (authInfo != null)
        {
            var attachmentAuthorUserProfile =
                (await _usersProxyClient.GetByIdsAsync(
                    new []{(attachment.UserProfileId == null ? 
                        0 : attachment.UserProfileId.Value).ToString()})).FirstOrDefault();
            result.CanDelete = CanDeleteAttachment(task, attachment, authInfo, attachmentAuthorUserProfile);
        }

        return result;
    }
    
    private bool CanDeleteAttachment(TaskModel task, 
        TaskAttachmentModel attachment, AuthInfo authInfo,
        UserProfileApiModel attachmentAuthorUserProfile)
    {
        if (!attachment.UserProfileId.HasValue || attachmentAuthorUserProfile == null)
        {
            return true;
        }

        var isFileOwn = attachment.UserProfileId.Value == authInfo.UserProfileId;
        return isFileOwn ||
               authInfo.Roles.Contains((int)Role.LegalHead) &&
               attachmentAuthorUserProfile.Roles.Contains((int)Role.LegalPerformer);
    }
}
