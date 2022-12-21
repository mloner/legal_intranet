using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Utg.Common.Models.Domain;

namespace Utg.LegalService.Common.Models.Domain
{
	[Index(nameof(UserProfileId))]
	public class UserProfileAgregate : BaseEntity
	{
		[Key]
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
		public bool IsRemoved { get; set; }
	}
}
