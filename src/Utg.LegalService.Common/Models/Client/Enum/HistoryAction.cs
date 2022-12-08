using System.ComponentModel.DataAnnotations;

namespace Utg.LegalService.Common.Models.Client.Enum
{
    public enum HistoryAction
    {
        [Display(Name = "")]
        None = 0,
        
        [Display(Name = "Задача создана")]
        Created = 1,
    }
}