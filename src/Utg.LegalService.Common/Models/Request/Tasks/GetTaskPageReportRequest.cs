namespace Utg.LegalService.Common.Models.Request.Tasks
{
    public class GetTaskPageReportRequest : GetTaskPageRequest
    {
        public int? TimeZoneOffsetMinutes { get; set; }
    }
}