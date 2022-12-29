using MediatR;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Api.Common.Models.UpdateModels.UserProfileUpdate;
using Utg.Common.Models;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdateUserProfile;

public class UpdateUserProfileAgregateCommand : IRequest<Result>
{
    public UpdateEvent<UserProfileUpdateEventModel> EventModel { get; set; }
}
