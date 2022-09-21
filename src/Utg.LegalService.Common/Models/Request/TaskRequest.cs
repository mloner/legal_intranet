using System;
using System.Collections.Generic;
using Utg.Common.Packages.Domain.Models.Request;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Request
{
    public class TaskRequest : PagedRequest
    {
        public IEnumerable<int> Statuses { get; set; }
        public IEnumerable<int> AuthorUserProfileIds { get; set; }
    }
}