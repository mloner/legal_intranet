using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client.Comment;
using Utg.LegalService.BL.Features.TaskChangeHistory.GetPage;
using Utg.LegalService.Common.Models.Request.TaskChangeHistory;
using Role = Utg.Common.Packages.Domain.Enums.Role;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TaskChangeHistoryController : BaseController
    {
        private readonly IMediator _mediator;
        
        public TaskChangeHistoryController(
            IUsersProxyClient usersClient,
            ILogger<BaseController> logger,
            IMediator mediator)
            : base(logger, usersClient)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskCommentModel>>> Get(
            [FromQuery] GetTaskChangeHistoryPageRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }

            var getHisComResp = request.Adapt<GetTaskChangeHistoryPageCommand>();
            var response = await _mediator.Send(getHisComResp, HttpContext.RequestAborted);
            return response.Success ? Ok(response.Data) : StatusCode(response.StatusCode, response.Message);

        }
    }
}