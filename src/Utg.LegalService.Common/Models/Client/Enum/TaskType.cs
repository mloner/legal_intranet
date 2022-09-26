using System.ComponentModel.DataAnnotations;

namespace Utg.LegalService.Common.Models.Client.Enum
{
    public enum TaskType
    {
        [Display(Name = "-")]
        None = 0,
        
        [Display(Name = "Судебная работа")]
        JudicialWork = 1,
        
        [Display(Name = "Претензионная работа")]
        ClaimWork = 2,
        
        [Display(Name = "Подготовка правовых заключений")]
        LegalOpinionsPreparation = 3,
        
        [Display(Name = "Подготовка доверенностей")]
        AttorneyPowerPreparation = 4,
        
        [Display(Name = "Работа с государственными органами")]
        GovernmentAgenciesWork = 5,
        
        [Display(Name = "Справочная информация")]
        ReferenceInformation = 6,
    }
}