using System.Collections.Generic;

namespace Utg.LegalService.BL.Features.Task.GetPage;

public class GetTaskPageCommandFilter
{
    public IEnumerable<int> Statuses { get; set; }
    public IEnumerable<int> AuthorUserProfileIds { get; set; }
    public string Search { get; set; }
}
