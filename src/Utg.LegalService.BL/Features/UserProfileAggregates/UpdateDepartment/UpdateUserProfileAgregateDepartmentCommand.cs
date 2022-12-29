using MediatR;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Api.Common.Models.UpdateModels.DepartmentUpdate;
using Utg.Common.Models;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateDepartment;

public class UpdateUserProfileAgregateDepartmentCommand : IRequest<Result>
{
    public UpdateEvent<DepartmentUpdateEventModel> EventModel { get; set; }
}
