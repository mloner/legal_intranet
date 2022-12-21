using System.ComponentModel.DataAnnotations;

namespace Utg.LegalService.Common.Models.UpdateModels
{
    public enum UpdateEventType
    {
        [Display(Name = "-")]
        None = 0,
    
        [Display(Name = "Create")]
        Create = 1,
    
        [Display(Name = "Update")]
        Update = 2,
    
        [Display(Name = "Delete")]
        Delete = 3,
    }
}
