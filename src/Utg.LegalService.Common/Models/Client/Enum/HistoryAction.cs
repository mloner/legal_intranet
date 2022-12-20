using System.ComponentModel.DataAnnotations;

namespace Utg.LegalService.Common.Models.Client.Enum
{
    public enum HistoryAction
    {
        [Display(Name = "")]
        None = 0,
        
        [Display(Name = "Создание")]
        Created = 1,
        
        [Display(Name = "Редактирование")]
        Changed = 2,
        
        [Display(Name = "Смена статуса")]
        StatusChanged = 3,
    }
}