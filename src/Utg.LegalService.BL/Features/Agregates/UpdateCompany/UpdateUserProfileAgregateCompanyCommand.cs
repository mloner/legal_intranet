using MediatR;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.CompanyUpdate;

namespace Utg.LegalService.BL.Features.Agregates.UpdateCompany;

public class UpdateUserProfileAgregateCompanyCommand : IRequest<Result>
{
    public UpdateEvent<CompanyUpdateEventModel> EventModel { get; set; }
}
