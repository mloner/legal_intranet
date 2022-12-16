﻿using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Utg.Common.Extensions;
using Utg.Common.Models;
using Utg.Common.Models.PaginationRequest;
using Utg.Common.Packages.Domain;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.BL.Features.AccessRights.Get;
using Utg.LegalService.BL.Features.Attachments.GetInfo;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL.Features.Task.GetPage;

public class GetTaskPageCommandHandler
    : IRequestHandler<GetTaskPageCommand, PaginationResult<TaskModel>>
{
    private readonly ILogger<GetTaskPageCommandHandler> _logger;
    private readonly UnitOfWork _uow;
    private readonly IMediator _mediator;
    private readonly IAgregateRepository _agregateRepository;

    public GetTaskPageCommandHandler(
        ILogger<GetTaskPageCommandHandler> logger,
        UnitOfWork uow,
        IMediator mediator,
        IAgregateRepository agregateRepository)
    {
        _logger = logger;
        _uow = uow;
        _mediator = mediator;
        _agregateRepository = agregateRepository;
    }

    public async System.Threading.Tasks.Task<PaginationResult<TaskModel>> Handle(
        GetTaskPageCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var predicate = GetPredicate(command.Filter, command);
            
            var tasks = await _uow.TaskItems.GetPagedAsync(
                predicate: predicate,
                orderByProperties: new List<OrderByPropertyDescriptor<Common.Models.Domain.Task>>()
                {
                    new OrderByPropertyDescriptor<Common.Models.Domain.Task>(task => task.CreationDateTime, 
                        EnumSortDirection.Desc)
                },
                take: command.Take,
                skip: command.Skip,
                cancellationToken: cancellationToken,
                x => x.TaskAttachments,
                x => x.TaskChangeHistories);

            var taskModels = tasks.Adapt<PaginationResult<TaskModel>>();

            var getAttUrlResp =
                await _mediator.Send(new GetAttachmentsInfoCommand()
                {
                    TaskModels = taskModels.Data
                }, cancellationToken);
            if (!getAttUrlResp.Success)
            {
                return PaginationResult<TaskModel>.Failed(getAttUrlResp);
            }
            taskModels.Data = getAttUrlResp.Data;

            var resultTaskModelsData = Enumerable.Empty<TaskModel>();
            foreach (var taskModel in taskModels.Data)
            {
                var getTaskAccRightsComResp =
                    await _mediator.Send(new GetTaskAccessRightsCommand()
                    {
                        Task = taskModel,
                        AuthInfo = command.AuthInfo
                    }, cancellationToken);
                if (!getTaskAccRightsComResp.Success)
                {
                    return PaginationResult<TaskModel>.Failed(getTaskAccRightsComResp);
                }

                taskModel.AccessRights = getTaskAccRightsComResp.Data;
                resultTaskModelsData = resultTaskModelsData.Append(taskModel);
            }

            taskModels.Data = resultTaskModelsData;
            
            return taskModels;
        }
        catch (Exception e)
        {
            const string failMsg = "Failed to get tasks.";
            _logger.LogError(e, "{@Msg} {@Command}", failMsg, command);

            return PaginationResult<TaskModel>.Internal(failMsg);
        }
    }

    private Expression<Func<Common.Models.Domain.Task, bool>> GetPredicate(
        GetTaskPageCommandFilter? filter,
        GetTaskPageCommand command)
    {
        Expression<Func<Common.Models.Domain.Task, bool>> predicate = q => true;
        if (filter == null)
            return predicate;

        predicate = predicate.And(FilterByRoles(command.AuthInfo));
        predicate = predicate.And(Filter(filter));
        predicate = predicate.And(Search(filter));

        return predicate;
    }

    private static Expression<Func<Common.Models.Domain.Task, bool>> FilterByRoles(
        AuthInfo authInfo)
    {
        Expression<Func<Common.Models.Domain.Task, bool>> predicate = q => true;
        
        if (authInfo.Roles.Contains((int)Role.IntranetUser)) {
            predicate = model => model.AuthorUserProfileId == authInfo.UserProfileId;
        }
        if (authInfo.Roles.Contains((int)Role.LegalPerformer))
        {
            predicate = model => model.PerformerUserProfileId == authInfo.UserProfileId 
                         || model.AuthorUserProfileId == authInfo.UserProfileId 
                         || (!model.PerformerUserProfileId.HasValue 
                             && StaticData.TypesToSelfAssign.Contains(model.Type));
        }
        if (authInfo.Roles.Contains((int)Role.LegalHead))
        {
            predicate = model => true;
        }
        

        predicate = predicate.And(x =>
            !(x.Status == TaskStatus.Draft &&
              x.AuthorUserProfileId != authInfo.UserProfileId));

        return predicate;
    }

    private Expression<Func<Common.Models.Domain.Task, bool>> Filter(
        GetTaskPageCommandFilter? filter)
    {
        Expression<Func<Common.Models.Domain.Task, bool>> predicate = q => true;

        if (filter.Statuses?.Any() == true)
        {
            predicate = predicate.And(x => filter.Statuses.Contains((int)x.Status));
        }
        else
        {
            predicate = predicate.And(x => x.Status != TaskStatus.Done);
        }

        if (filter.AuthorUserProfileIds?.Any() == true)
        {
            predicate =
                predicate.And(x => filter.AuthorUserProfileIds.Contains(x.AuthorUserProfileId));
        }

        if (filter.MoveToWorkDateTimeFrom.HasValue)
        {
            predicate = predicate.And(
                x => x.TaskChangeHistories
                         .OrderByDescending(his => his.DateTime)
                         .FirstOrDefault(
                             his => 
                                 his.TaskStatus == TaskStatus.InWork &&
                                 his.DateTime >= filter.MoveToWorkDateTimeFrom.Value) != null);
        }
        
        if (filter.MoveToWorkDateTimeTo.HasValue)
        {
            predicate = predicate.And(
                x => x.TaskChangeHistories
                         .OrderByDescending(his => his.DateTime)
                         .FirstOrDefault(
                             his => 
                                 his.TaskStatus == TaskStatus.InWork &&
                                 his.DateTime <= filter.MoveToWorkDateTimeTo.Value) != null);
        }

        return predicate;
    }

    private Expression<Func<Common.Models.Domain.Task, bool>> Search(
        GetTaskPageCommandFilter? filter)
    {
        Expression<Func<Common.Models.Domain.Task, bool>> predicate = q => true;

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var ftsQuery = Util.GetFullTextSearchQuery(filter.Search);
            var ilikeQuery = $"%{filter.Search}%";
            var searchNormalized = Util.Normalize(filter.Search);
            var taskTypesNormalized = EnumExtensions.GetEnumValuesWithoutDefault<TaskType>()
                .Select(x =>
                {
                    x.Text = Util.Normalize(x.Text);
                    return x;
                });

            var userProfileIds = _agregateRepository.Get()
                .Where(x =>
                    EF.Functions.ILike(x.FullName, ilikeQuery)
                    || EF.Functions.ToTsVector(Const.PgFtsConfig, x.FullName)
                        .Matches(EF.Functions.PlainToTsQuery(Const.PgFtsConfig, ftsQuery)))
                .Select(x => x.UserProfileId);

            var neededTaskTypes = taskTypesNormalized
                .Where(x => x.Text.Contains(searchNormalized))
                .Select(x => (TaskType)x.Id);

            predicate = predicate.And(x
                => userProfileIds.Contains(x.AuthorUserProfileId)
                   || 
                   x.PerformerUserProfileId.HasValue && 
                   userProfileIds.Contains(x.PerformerUserProfileId.Value)
                    ||
                   neededTaskTypes.Contains(x.Type)
                   ||
                   EF.Functions.ILike(x.Description, ilikeQuery)
                   || EF.Functions.ToTsVector(Const.PgFtsConfig, x.Description)
                       .Matches(EF.Functions.PlainToTsQuery(Const.PgFtsConfig, ftsQuery)));
        }

        return predicate;
    }
}
