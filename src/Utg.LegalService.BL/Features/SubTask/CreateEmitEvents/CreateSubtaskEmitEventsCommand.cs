using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.SubTask.CreateEmitEvents;

public class CreateSubtaskEmitEventsCommand :  IRequest<Result>
{
    public TaskModel Task { get; set; }
    public AuthInfo AuthInfo { get; set; }
}
