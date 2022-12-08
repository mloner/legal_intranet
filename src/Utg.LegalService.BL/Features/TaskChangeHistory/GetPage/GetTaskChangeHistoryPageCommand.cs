using MediatR;
using Utg.Common.Models;
using Utg.Common.Models.PaginationRequest;
using Utg.LegalService.Common.Models.Client.TaskChangeHistory;

namespace Utg.LegalService.BL.Features.TaskChangeHistory.GetPage;

public class GetTaskChangeHistoryPageCommand 
    : PaginationContext<GetTaskChangeHistoryPageCommandFilter>, 
        IRequest<PaginationResult<TaskChangeHistoryModel>>
{
    public int? TaskId { get; set; }
}
