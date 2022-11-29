using MediatR;
using Utg.Common.Models;
using Utg.Common.Models.PaginationRequest;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Task.GetPage;

public class GetTaskPageCommand 
    : PageContext<GetTaskPageCommandFilter>, IRequest<PaginationResult<TaskModel>>
{
    public AuthInfo AuthInfo { get; set; }
}
