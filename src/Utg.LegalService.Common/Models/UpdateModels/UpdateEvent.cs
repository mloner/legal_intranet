namespace Utg.LegalService.Common.Models.UpdateModels
{
    public class UpdateEvent<T>
    {
        public UpdateEventType Type { get; set; }
        public T Data { get; set; }
    }
}
