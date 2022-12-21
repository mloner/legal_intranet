using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;
using Role = Utg.Common.Packages.Domain.Enums.Role;

namespace Utg.LegalService.API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected ILogger logger;
        private readonly IUsersProxyClient usersClient;
        protected static readonly string CurrentUserProfileCookie = "X-Current-UserProfile";


        public BaseController(
                ILogger logger,
                IUsersProxyClient usersClient)
        {
            this.logger = logger;
            this.usersClient = usersClient;
        }

        protected async Task<bool> CanGo(params Role[] allowedRoles)
        {
            var parsedRoles = allowedRoles.Select(role => (int)role);
            var currentUserRoles = await GetCurrentUserRoles();
            return currentUserRoles.Any(currentUserRole => parsedRoles.Contains(currentUserRole));
        }

        protected string GetErrors()
        {
            StringBuilder modelErrors = new StringBuilder();
            foreach (var modelState in ModelState.Values)
            {
                foreach (var modelError in modelState.Errors)
                {
                    modelErrors.Append(modelError.ErrorMessage);
                }
            }
            return modelErrors.ToString();
        }

        protected IActionResult BadRequestWithError(string error)
        {
            var details = new ProblemDetails { Detail = error, Status = 400 };
            return BadRequest(details);
        }

        protected async Task<int?> GetCurrentUserProfileId()
        {
            var userprofile = await this.GetUserProfile();
            return userprofile?.Id;
        }

        protected async Task<IEnumerable<int>> GetCurrentUserRoles()
        {
            var userprofile = await this.GetUserProfile();
            return userprofile?.Roles.Select(x => (int)x) ?? Enumerable.Empty<int>();
        }

        protected async Task<AuthInfo> GetAuthInfo()
        {
            var userprofile = await this.GetUserProfile();
            var auth = Request.Headers["Authorization"].ToString();

            return new AuthInfo
            {
                UserId = userprofile.UserId,
                UserProfileId = userprofile.Id,
                AuthToken = auth,
                Roles = userprofile.Roles.Select(x => (int)x),
                FullNameInitials = userprofile.FullNameInitials,
                Name = userprofile.Name,
                Surname = userprofile.Surname,
                Patronymic = userprofile.Patronymic,
            };
        }

        private Task<UserProfileViewModel> GetUserProfile()
        {
            var currentUserProfileId = this.HttpContext.Request.Cookies[CurrentUserProfileCookie];
            return this.usersClient.GetCurrentUserAsync(currentUserProfileId);
        }
    }
}