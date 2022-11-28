using System.Collections.Generic;
using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;

namespace Utg.LegalService.BL.Features.Agregates.GetList;

public class GetListUserProfileAgregatesCommand 
    : IRequest<Result<IEnumerable<UserProfileAgregateModel>>>
{
    public IEnumerable<int> UserProfileIds { get; set; }
}
