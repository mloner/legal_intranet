using MediatR;
using Utg.Common.Models;
using Utg.Common.Models.PagedRequest;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.Task.GetPage;

public class GetTaskPageCommand 
    : PageContext<GetTaskPageCommandFilter>, IRequest<PagedResult<TaskModel>>
{
    public AuthInfo AuthInfo { get; set; }
}
