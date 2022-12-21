using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.UpdateModels;
using Utg.LegalService.Common.Models.UpdateModels.UserProfileUpdate;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateUserProfile;

public class UpdateUserProfileAgregateCommand : IRequest<Result>
{
    public UpdateEvent<UserProfileUpdateEventModel> EventModel { get; set; }
}
