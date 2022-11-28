using System;
using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Task.NotifyExpiredSoonTasks;

public class NotifyExpiredSoonTasksCommand :  IRequest<Result>
{
    public DateTime DateTime { get; set; }
}
