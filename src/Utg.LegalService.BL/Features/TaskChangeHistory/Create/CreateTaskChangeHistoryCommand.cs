using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client.Enum;
using Utg.LegalService.Common.Models.Client.TaskChangeHistory;

namespace Utg.LegalService.BL.Features.TaskChangeHistory.Create;

public class CreateTaskChangeHistoryCommand 
    : IRequest<Result<TaskChangeHistoryModel>>
{
    public int TaskId { get; set; }
    public int UserProfileId { get; set; }
    public HistoryAction HistoryAction { get; set; }
}
