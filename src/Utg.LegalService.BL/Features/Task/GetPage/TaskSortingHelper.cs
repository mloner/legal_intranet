using System;
using System.Collections.Generic;
using System.Linq;
using Utg.Common.Models.PaginationRequest;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Task.GetPage;

internal static class TaskSortingHelper
{
    internal static IEnumerable<TaskModel> Sort(
        this IEnumerable<TaskModel> models, GetTaskPageCommand command)
    {
        var sortDirEnum = EnumExtensions.GetEnumValue<EnumSortDirection>(command.SortDirection);
        switch (command.SortBy)
        {
            case nameof(TaskModel.ParentTaskId):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.ParentTaskId)
                    : models.OrderBy(x => x.ParentTaskId);
                break;
            case nameof(TaskModel.Status):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.StatusName)
                    : models.OrderBy(x => x.StatusName);
                break;
            case nameof(TaskModel.Type):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.TypeName)
                    : models.OrderBy(x => x.TypeName);
                break;
            case nameof(TaskModel.Description):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.Description)
                    : models.OrderBy(x => x.Description);
                break;
            case nameof(TaskModel.AuthorFullName):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.AuthorFullName)
                    : models.OrderBy(x => x.AuthorFullName);
                break;
            case nameof(TaskModel.CreationDateTime):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.CreationDateTime)
                    : models.OrderBy(x => x.CreationDateTime);
                break;
            case nameof(TaskModel.PerformerFullName):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.PerformerFullName)
                    : models.OrderBy(x => x.PerformerFullName);
                break;
            case nameof(TaskModel.DeadlineDateTime):
                models = sortDirEnum == EnumSortDirection.Desc
                    ? models.OrderByDescending(x => x.DeadlineDateTime)
                    : models.OrderBy(x => x.DeadlineDateTime);
                break;
            case null:
                models = models.OrderByDescending(x => x.CreationDateTime);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command.SortBy));
        }

        return models;
    }
}