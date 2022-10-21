using System;

namespace Utg.LegalService.Common.Models.Client;

public class UserProfileAgregateModel
{
    public int Id { get; set; }
    public int UserProfileId { get; set; }
    public int UserId { get; set; }
    public int Status { get; set; }
    public int Type { get; set; }
    public string TabN { get; set; }
    public DateTime? DismissalDate { get; set; }
    public int? CompanyId { get; set; }
    public string CompanyName { get; set; }
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int? PositionId { get; set; }
    public string PositionName { get; set; }
    public string FullName { get; set; }
    public int? ManagerPositionId { get; set; }
    public bool IsRemoved { get; set; }
    public int? HeadUserProfileId { get; set; }
}