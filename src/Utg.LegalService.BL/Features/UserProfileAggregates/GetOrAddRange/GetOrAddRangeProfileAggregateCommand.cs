using System;
using System.Collections.Generic;
using MediatR;
using Utg.Common.Models;
using Utg.LegalService.Common.Models.Client;

namespace Utg.LegalService.BL.Features.UserProfileAggregates.GetOrAddRange;

public class GetOrAddRangeProfileAggregateCommand 
    : IRequest<ResultList<UserProfileAgregateModel>>
{
    public List<int>? UserProfileIds { get; set; }

    public List<int>? UserIds { get; set; }

    public List<string>? TabNs { get; set; }

    public List<Guid>? PersonIds { get; set; }
    public List<Guid>? OuterIds { get; set; }
}
