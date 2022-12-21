using System;
using System.Collections.Generic;

namespace Utg.LegalService.Common.Models.UpdateModels.UserProfileUpdate
{
    public class UserProfileUpdateEventModel
    {
        public int UserProfileId { get; set; }
        public int UserId { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string? TabN { get; set; }
        public DateTime? DismissalDate { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? FullName { get; set; }
        public bool IsRemoved { get; set; }
        public int? ChiefId { get; set; }
        public Guid? OuterId { get; set; }
        public Guid? PersonId { get; set; }
        public IEnumerable<UserProfileStatusUpdateEventModel> UserProfileStatuses { get; set; }
    }
}
