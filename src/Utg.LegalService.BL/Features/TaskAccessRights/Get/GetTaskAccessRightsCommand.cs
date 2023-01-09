using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.TaskAccessRights.Get;

public class GetTaskAccessRightsCommand 
    : IRequest<Result<Common.Models.Client.Task.TaskAccessRights>>
{
    public TaskModel Task { get; set; }
    public AuthInfo AuthInfo { get; set; }
}
