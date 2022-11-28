using MediatR;
using Utg.Common.Models;
using Utg.Common.Packages.Domain.Models.UpdateModels;
using Utg.Common.Packages.Domain.Models.UpdateModels.PositionUpdate;

namespace Utg.LegalService.BL.Features.Agregates.UpdatePosition;

public class UpdateUserProfileAgregatePositionCommand : IRequest<Result>
{
    public UpdateEvent<PositionUpdateEventModel> EventModel { get; set; }
}
