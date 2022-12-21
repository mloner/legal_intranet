using System;

namespace Utg.LegalService.Common.Models.UpdateModels.PositionUpdate
{
    public class PositionUpdateEventModel
    {
        public int Id { get; set; }
        public Guid OuterId { get; set; }
        public Guid? GlobalOuterId { get; set; }
        public Guid? OuterDepartmentId { get; set; }
        public float? Rate { get; set; }
        public bool IsInternal { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public bool Deleted { get; set; }
        public DateTime? CloseDate { get; set; }
        public DateTime? AcceptDate { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
