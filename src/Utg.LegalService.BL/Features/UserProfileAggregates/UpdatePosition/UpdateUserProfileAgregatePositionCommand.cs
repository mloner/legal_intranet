using MediatR;
using Utg.Api.Common.Models.UpdateModels;
using Utg.Api.Common.Models.UpdateModels.PositionUpdate;
using Utg.Common.Models;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.UpdatePosition;

public class UpdateUserProfileAgregatePositionCommand : IRequest<Result>
{
    public UpdateEvent<PositionUpdateEventModel> EventModel { get; set; }
}
