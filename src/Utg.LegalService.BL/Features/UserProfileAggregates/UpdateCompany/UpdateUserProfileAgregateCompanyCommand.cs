using MediatR;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Api.Common.Models.UpdateModels.CompanyUpdate;
using Utg.Common.Models;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateCompany;

public class UpdateUserProfileAgregateCompanyCommand : IRequest<Result>
{
    public UpdateEvent<CompanyUpdateEventModel> EventModel { get; set; }
}
