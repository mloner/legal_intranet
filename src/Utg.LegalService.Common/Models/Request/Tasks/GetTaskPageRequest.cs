using System;
using System.Collections.Generic;
using Utg.Common.Packages.Domain.Models.Request;

namespace Utg.LegalService.Common.Models.Request.Tasks
{
    public class GetTaskPageRequest : PagedRequest
    {
        public IEnumerable<int> Statuses { get; set; }
        public IEnumerable<int> AuthorUserProfileIds { get; set; }
        public DateTime? MoveToWorkDateTimeFrom { get; set; }
        public DateTime? MoveToWorkDateTimeTo { get; set; }
    }
}