using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utg.Common.Extensions;
using Utg.Common.Models;
using Utg.Common.Packages.Domain;
using Utg.Common.Packages.Domain.Enums;
using Utg.LegalService.BL.Features.AccessRights.Get;
using Utg.LegalService.BL.Features.Agregates.GetList;
using Utg.LegalService.BL.Features.Attachments.GetInfo;
using Utg.LegalService.BL.Features.Task.GetPage;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Client.TaskChangeHistory;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.TaskChangeHistory.GetPage;

public class GetTaskChangeHistoryPageCommandhandler
    : IRequestHandler<GetTaskChangeHistoryPageCommand,
        PaginationResult<TaskChangeHistoryModel>>
{
    private readonly ILogger<GetTaskChangeHistoryPageCommandhandler> _logger;
    private readonly UnitOfWork _uow;
    private readonly IMediator _mediator;

    public GetTaskChangeHistoryPageCommandhandler(
        ILogger<GetTaskChangeHistoryPageCommandhandler> logger,
        UnitOfWork uow,
        IMediator mediator)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
    }

    public async System.Threading.Tasks.Task<PaginationResult<TaskChangeHistoryModel>> Handle(
        GetTaskChangeHistoryPageCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var predicate = GetPredicate(command.Filter);

            var historyItems = await _uow.TaskChangeHistoryItems.GetPagedAsync(
                predicate: predicate,
                pageSize: command.PageSize,
                pageIndex: command.PageIndex,
                cancellationToken: cancellationToken);
            
            var taskModels = historyItems.Adapt<PaginationResult<TaskChangeHistoryModel>>();
            
            var userProfileIds = taskModels.Data.Select(x => x.UserProfileId);
            var getAgregatesCommand = new GetListUserProfileAgregatesCommand()
            {
                UserProfileIds = userProfileIds
            };
            var getAgregatesCommandResponse = 
                await _mediator.Send(getAgregatesCommand, cancellationToken);
            if (!getAgregatesCommandResponse.Success)
            {
                return PaginationResult<TaskChangeHistoryModel>.Failed(getAgregatesCommandResponse);
            }
            var upas = getAgregatesCommandResponse.Data;

            taskModels.Data = taskModels.Data.Select(x =>
            {
                var upa = upas.FirstOrDefault(y => y.UserProfileId == x.UserProfileId);
                x.UserProfileFullName = upa?.FullName;
                
                return x;
            });

            return taskModels;
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to get tasks.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);

            return PaginationResult<TaskChangeHistoryModel>.Internal(failMsg);
        }
    }

    private Expression<Func<Common.Models.Domain.TaskChangeHistory, bool>> GetPredicate(
        GetTaskChangeHistoryPageCommandFilter? filter)
    {
        Expression<Func<Common.Models.Domain.TaskChangeHistory, bool>> predicate = 
            q => true;
        if (filter == null)
            return predicate;

        predicate = predicate.And(Filter(filter));

        return predicate;
    }
    

    private Expression<Func<Common.Models.Domain.TaskChangeHistory, bool>> Filter(
        GetTaskChangeHistoryPageCommandFilter? filter)
    {
        Expression<Func<Common.Models.Domain.TaskChangeHistory, bool>> predicate = q => true;

        if (filter.TaskId.HasValue)
        {
            predicate = predicate.And(x => x.TaskId == filter.TaskId.Value);
        }

        return predicate;
    }
}
