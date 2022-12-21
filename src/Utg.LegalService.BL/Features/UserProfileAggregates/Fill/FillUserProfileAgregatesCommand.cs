using MediatR;
using Utg.Common.Models;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.Fill;

public class FillUserProfileAgregatesCommand : IRequest<Result>
{
    public bool Full { get; set; } 
}
