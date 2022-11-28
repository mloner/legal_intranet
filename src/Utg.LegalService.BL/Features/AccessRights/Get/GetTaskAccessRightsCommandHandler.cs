using System;
using System.Linq;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Enums;
using Utg.LegalService.BL.Features.Task.Get;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.AccessRights.Get;

public class GetTaskAccessRightsCommandHandler 
    : IRequestHandler<GetTaskAccessRightsCommand, Result<TaskAccessRights>>
{
    private readonly ILogger<GetTaskAccessRightsCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly UnitOfWork _uow;

    public GetTaskAccessRightsCommandHandler(
        ILogger<GetTaskAccessRightsCommandHandler> logger,
        UnitOfWork uow, 
        IMediator mediator)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task<Result<TaskAccessRights>> Handle(
        GetTaskAccessRightsCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var ar = new TaskAccessRights
            {
                IsPerformerAvailable = IsPerformerAvailable(command.AuthInfo),
                CanShowDetails = CanShowDetails(command.AuthInfo),
                CanEdit = CanEdit(command.Task, command.AuthInfo),
                CanDelete = CanDelete(command.Task, command.AuthInfo),
                CanMakeReport = CanMakeReport(command.Task, command.AuthInfo),
                CanPerform = CanPerform(command.Task, command.AuthInfo),
                CanReview = CanReview(command.Task, command.AuthInfo)
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
    
    private static bool IsPerformerAvailable(AuthInfo authInfo)
    {
        return authInfo.Roles.Contains((int)Role.LegalHead);
    }
    
    private static bool CanShowDetails(AuthInfo authInfo)
    {
        return new int[] { (int)Role.LegalHead, (int)Role.IntranetUser, (int)Role.LegalPerformer }
            .Intersect(authInfo.Roles)
            .Any();
    }
    
    private static bool CanEdit(TaskModel model, AuthInfo authInfo)
        => authInfo.Roles.Contains((int)Role.IntranetUser)
           && model.Status == TaskStatus.Draft ||
           authInfo.Roles.Contains((int)Role.LegalHead)
           && model.Status == TaskStatus.New;


    private static bool CanDelete(TaskModel model, AuthInfo authInfo)
    {
        return authInfo.Roles.Contains((int)Role.IntranetUser) &&
               model.Status == TaskStatus.Draft;
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
