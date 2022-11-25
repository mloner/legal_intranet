using MediatR;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.PositionUpdate;
using Utg.Common.Packages.Domain.Models.UpdateModels.UserProfileUpdate;

namespace Utg.LegalService.BL.Features.Agregates.UpdateUserProfile;

public class UpdateUserProfileAgregateCommand : IRequest<Result>
{
    public UpdateEvent<UserProfileUpdateEventModel> EventModel { get; set; }
}
