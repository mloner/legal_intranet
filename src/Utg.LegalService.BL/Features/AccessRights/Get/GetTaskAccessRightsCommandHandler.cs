using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Enums;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Dal;
using TaskStatus = Utg.LegalService.Common.Models.Client.Enum.TaskStatus;

namespace Utg.LegalService.BL.Features.AccessRights.Get;

public class GetTaskAccessRightsCommandHandler
    : IRequestHandler<GetTaskAccessRightsCommand, Result<TaskAccessRights>>
{
    private readonly ILogger<GetTaskAccessRightsCommandHandler> _logger;
    private readonly IUnitOfWork _uow;

    public GetTaskAccessRightsCommandHandler(
        ILogger<GetTaskAccessRightsCommandHandler> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async System.Threading.Tasks.Task<Result<TaskAccessRights>> Handle(
        GetTaskAccessRightsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var ar = new TaskAccessRights
            {
                CanShowDetails = CanShowDetails(command.Task, command.AuthInfo),
                CanEdit = CanEdit(command.Task, command.AuthInfo),
                CanDelete = await CanDelete(command.Task, command.AuthInfo),
                CanMakeReport = CanMakeReport(command.Task, command.AuthInfo),
                CanPerform = CanPerform(command.Task, command.AuthInfo),
                CanReview = CanReview(command.Task, command.AuthInfo),
                HasShortCycle = HasShortCycle(command.Task),
                CanMoveToDone = await CanMoveToDone(command.Task),
                CanCreateSubtask = await CanCreateSubtask(command.Task,command.AuthInfo)
            };

            return Result<TaskAccessRights>.Ok(ar);
        }
        catch (Exception e)
        {
            var failMsg = "Failed to get task access rights.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);

            return Result<TaskAccessRights>.Internal(failMsg);
        }
    }

    private static async Task<bool?> CanCreateSubtask(TaskModel taskModel,
        AuthInfo authInfo)
    {
        return !taskModel.ParentTaskId.HasValue
            && taskModel.Status != TaskStatus.Draft
            && new[] {
                (int)Role.LegalHead, 
                (int)Role.LegalPerformer
            }.Intersect(authInfo.Roles).Any();
    }

    private async Task<bool> CanMoveToDone(TaskModel taskModel)
    {
        var hasUndoneChildTasks =
            await _uow.TaskRepository.AnyAsync(x =>
                    x.ParentTaskId == taskModel.Id &&
                    x.Status != TaskStatus.Done);
        return !hasUndoneChildTasks;
    }

    private static bool HasShortCycle(TaskModel task)
            => StaticData.TypesToSelfAssign.Contains(task.Type) || task.ParentTaskId.HasValue;

   
    private static bool CanShowDetails(TaskModel model, AuthInfo authInfo)
    {
        return new int[] { (int)Role.LegalHead, (int)Role.LegalPerformer }
            .Intersect(authInfo.Roles)
            .Any() || authInfo.UserProfileId == model.AuthorUserProfileId;
    }

    private static bool CanEdit(TaskModel model, AuthInfo authInfo)
        => (authInfo.Roles.Contains((int)Role.IntranetUser)
           && model.Status == TaskStatus.Draft && model.AuthorUserProfileId == authInfo.UserProfileId)
            || authInfo.Roles.Contains((int)Role.LegalHead) && model.Status == TaskStatus.New
            || authInfo.Roles.Contains((int)Role.LegalPerformer)
                && model.Status == TaskStatus.New && StaticData.TypesToSelfAssign.Contains(model.Type);

    private async Task<bool> CanDelete(TaskModel model, AuthInfo authInfo)
    {
        var hasChildTasks = await _uow.TaskRepository.AnyAsync(x => x.ParentTaskId == model.Id);
        return authInfo.UserProfileId == model.AuthorUserProfileId &&
               !hasChildTasks;
    }

    private static bool CanMakeReport(TaskModel model, AuthInfo authInfo)
    {
        return authInfo.Roles.Contains((int)Role.LegalHead);
    }

    private static bool CanPerform(TaskModel model, AuthInfo authInfo)
        => authInfo.Roles.Contains((int)Role.LegalPerformer) &&
           model.Status == TaskStatus.InWork &&
           model.PerformerUserProfileId == authInfo.UserProfileId;

    private static bool CanReview(TaskModel model, AuthInfo authInfo)
        => authInfo.Roles.Contains((int)Role.LegalHead) &&
           model.Status == TaskStatus.UnderReview;
}
