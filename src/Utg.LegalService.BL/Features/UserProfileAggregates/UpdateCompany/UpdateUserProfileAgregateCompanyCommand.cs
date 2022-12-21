using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.UpdateModels;
using Utg.LegalService.Common.Models.UpdateModels.CompanyUpdate;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateCompany;

public class UpdateUserProfileAgregateCompanyCommand : IRequest<Result>
{
    public UpdateEvent<CompanyUpdateEventModel> EventModel { get; set; }
}
