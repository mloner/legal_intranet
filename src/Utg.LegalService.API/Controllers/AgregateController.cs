using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AgregateController : BaseController
    {
        private readonly IAgregateService _agregateService;
        public AgregateController(
            ILogger<BaseController> logger,
            IUsersProxyClient usersClient,
            IAgregateService agregateService)
            : base(logger, usersClient)
        {
            _agregateService = agregateService;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> FillUserProfiles()
        {
            await _agregateService.FillUserProfiles();
            return Ok();
        }
    }
}
