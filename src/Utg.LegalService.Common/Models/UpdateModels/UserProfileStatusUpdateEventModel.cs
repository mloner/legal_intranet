using System;
using Utg.Api.Common.Models.Enums;

namespace Utg.LegalService.Common.Models.UpdateModels;

public class UserProfileStatusUpdateEventModel
{
    public int? Id { get; set; }

    public int? UserProfileId { get; set; }
    
    public bool? Removed { get; set; }

    public UserProfileExtendedStatus? Status { get; set; }

    public DateTime? ExpiredAfter { get; set; }
}