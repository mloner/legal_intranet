using System;

namespace Utg.LegalService.Common.Models.UpdateModels.CompanyUpdate
{
    public class CompanyUpdateEventModel
    {
        public int Id { get; set; }
        public Guid? OuterId { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public int? HeaderCode { get; set; }
        public bool Deleted { get; set; }
        public CompanyUpdateType Type { get; set; }
        public DateTime? CreatedUTC { get; set; }
        public DateTime? UpdatedUTC { get; set; }
    }
}
