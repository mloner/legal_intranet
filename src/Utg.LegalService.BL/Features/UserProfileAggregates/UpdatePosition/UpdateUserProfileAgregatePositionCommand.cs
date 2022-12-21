using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.UpdateModels;
using Utg.LegalService.Common.Models.UpdateModels.PositionUpdate;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdatePosition;

public class UpdateUserProfileAgregatePositionCommand : IRequest<Result>
{
    public UpdateEvent<PositionUpdateEventModel> EventModel { get; set; }
}
