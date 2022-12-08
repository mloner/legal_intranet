using Utg.Common.Packages.Domain.Models.Request;

namespace Utg.LegalService.Common.Models.Request.TaskChangeHistory;

public class GetTaskChangeHistoryPageRequest : PagedRequest
{
    public int? TaskId { get; set; }
}