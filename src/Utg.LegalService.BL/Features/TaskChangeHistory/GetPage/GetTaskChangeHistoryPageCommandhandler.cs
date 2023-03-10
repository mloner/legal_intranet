using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Utg.Common.Extensions;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.TaskChangeHistory;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.TaskChangeHistory.GetPage;

public class GetTaskChangeHistoryPageCommandhandler
    : IRequestHandler<GetTaskChangeHistoryPageCommand,
        PaginationResult<TaskChangeHistoryModel>>
{
    private readonly ILogger<GetTaskChangeHistoryPageCommandhandler> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IMediator _mediator;

    public GetTaskChangeHistoryPageCommandhandler(
        ILogger<GetTaskChangeHistoryPageCommandhandler> logger,
        IUnitOfWork uow, 
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

            var historyItems = await _uow.TaskChangeHistoryRepository.GetPagedAsync(
                predicate: predicate,
                pageSize: command.PageSize,
                pageIndex: command.PageIndex,
                cancellationToken: cancellationToken);
            
            var taskModels = historyItems.Adapt<PaginationResult<TaskChangeHistoryModel>>();
            
            var userProfileIds = taskModels.Data.Select(x => x.UserProfileId);
            var upas = _uow.UserProfileAgregatesRepository
                .GetQuery(x => userProfileIds.Contains(x.UserProfileId), null);

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
