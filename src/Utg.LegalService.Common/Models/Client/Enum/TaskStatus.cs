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
        New = 2,
        
        [Display(Name = "В работе")]
        InWork = 3,
        
        [Display(Name = "На проверке")]
        UnderReview = 4,
        
        [Display(Name = "Выполнено")]
        Done = 5
    }
}