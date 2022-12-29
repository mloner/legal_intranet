using System;
using MediatR;
using Utg.Common.Models;

namespace Utg.LegalService.BL.Features.Task.NotifyExpiredSoonTasks;

public class NotifyExpiredSoonTasksCommand :  IRequest<Result>
{
    public DateTime DateTime { get; set; }
}
