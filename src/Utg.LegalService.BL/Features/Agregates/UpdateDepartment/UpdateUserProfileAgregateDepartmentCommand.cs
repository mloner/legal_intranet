using MediatR;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.DepartmentUpdate;

namespace Utg.LegalService.BL.Features.Agregates.UpdateDepartment;

public class UpdateUserProfileAgregateDepartmentCommand : IRequest<Result>
{
    public UpdateEvent<DepartmentUpdateEventModel> EventModel { get; set; }
}
