using System.ComponentModel.DataAnnotations;

namespace Utg.LegalService.Common.Models.Client.Enum
{
    public enum TaskStatus
    {
        [Display(Name = "-")]
        None = 0,
        
        [Display(Name = "Черновик")]
        Draft = 1,
        
        [Display(Name = "Новый")]
        New = 2
    }
}