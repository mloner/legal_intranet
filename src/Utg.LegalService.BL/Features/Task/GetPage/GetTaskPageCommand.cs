using MediatR;
using Utg.Common.Models;
using Utg.Common.Models.PaginationRequest;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Task.GetPage;

public class GetTaskPageCommand 
    : PaginationContext<GetTaskPageCommandFilter>, IRequest<PaginationResult<TaskModel>>
{
    public AuthInfo AuthInfo { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
