using System;
using System.Collections.Generic;
using Utg.Common.Packages.Domain.Helpers;
using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.Common.Models.Client.Task
{
    public class SubtaskModel : TaskModel
    {
        public int? ParentTaskId { get; set; }
    }
}