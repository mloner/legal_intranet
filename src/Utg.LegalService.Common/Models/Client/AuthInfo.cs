using System.Collections.Generic;

namespace Utg.LegalService.Common.Models.Client
{
    public class AuthInfo
    {
        public int UserId { get; set; }
        public int UserProfileId { get; set; }
        public string AuthToken { get; set; }
        public IEnumerable<int> Roles { get; set; }
        public string FullNameInitials { get; set; }
        public string Patronymic { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }

        public string FullName => $"{Surname} {Name} {Patronymic}";
    }
}