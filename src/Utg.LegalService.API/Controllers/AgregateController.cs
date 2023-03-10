using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Models.Mvc.Core.Response;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.BL.Features.UserProfileAggregates.Delete;
using Utg.LegalService.BL.Features.UserProfileAggregates.Fill;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]")]
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    public class AgregateController : BaseController
    {
        private readonly IMediator _mediator;
        
        public AgregateController(
            ILogger<BaseController> logger,
            IUsersProxyClient usersClient,
            IMediator mediator)
            : base(logger, usersClient)
        {
            _mediator = mediator;
        }


        [HttpPost("fillUserProfiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<ActionResult> FillUserProfiles(FillUserProfileAgregatesCommand command)
        {
            var response = await _mediator.Send(command, HttpContext.RequestAborted);
            return response.HttpResponse();
        }
        
        [HttpDelete("deleteAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<ActionResult> DeleteAll(DeleteAllAgregatesCommand command)
        {
            var response = await _mediator.Send(command, HttpContext.RequestAborted);
            return response.HttpResponse();
        }
    }
}
