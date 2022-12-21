using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.GetOrAdd;

public class GetOrAddProfileAggregateCommand : IRequest<Result<UserProfileAgregateModel>>
{
    public int UserProfileId { get; set; }
}
