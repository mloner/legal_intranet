using Utg.LegalService.Common.Models.Client.Enum;

namespace Utg.LegalService.BL
{
    public  class StaticData
    {
        public static readonly TaskType[] TypesToSelfAssign = { TaskType.AttorneyPowerPreparation, TaskType.GovernmentAgenciesWork, TaskType.ReferenceInformation };
    }
}
