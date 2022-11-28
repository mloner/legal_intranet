using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.AccessRights.Get;

public class GetTaskAccessRightsCommand :  IRequest<Result<TaskAccessRights>>
{
    public TaskModel Task { get; set; }
    public AuthInfo AuthInfo { get; set; }
}
