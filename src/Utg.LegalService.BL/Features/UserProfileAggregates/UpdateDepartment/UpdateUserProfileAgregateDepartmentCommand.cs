using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.UpdateModels;
using Utg.LegalService.Common.Models.UpdateModels.DepartmentUpdate;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateDepartment;

public class UpdateUserProfileAgregateDepartmentCommand : IRequest<Result>
{
    public UpdateEvent<DepartmentUpdateEventModel> EventModel { get; set; }
}
