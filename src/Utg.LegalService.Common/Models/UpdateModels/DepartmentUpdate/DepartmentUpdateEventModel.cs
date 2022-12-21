using System;

namespace Utg.LegalService.Common.Models.UpdateModels.DepartmentUpdate
{
    public class DepartmentUpdateEventModel
    {
        public int Id { get; set; }
        public Guid? OuterId { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public bool IsInternal { get; set; }
        public bool Active { get; set; }
        public int? ParentDepartmentId { get; set; }
        public string HeadOffice { get; set; }
        public int? ManagerPositionId { get; set; }
        public bool Deleted { get; set; }
        public bool Canceled { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        public int? CompanyId { get; set; }
    }
}
