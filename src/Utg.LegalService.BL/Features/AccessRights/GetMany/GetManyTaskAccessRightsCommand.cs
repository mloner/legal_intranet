using System.Collections.Generic;
using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;

namespace Utg.LegalService.BL.Features.AccessRights.GetMany;

public class GetManyTaskAccessRightsCommand :  IRequest<Result<IEnumerable<TaskAccessRights>>>
{
    public IEnumerable<TaskModel> Tasks { get; set; }
    public AuthInfo AuthInfo { get; set; }
}
