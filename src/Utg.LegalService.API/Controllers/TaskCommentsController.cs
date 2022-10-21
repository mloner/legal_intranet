using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.Common.Packages.Domain.Models.Enum;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request;
using Utg.LegalService.Common.Models.Request.TaskComments;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TaskCommentsController : BaseController
    {
        private readonly ITaskCommentService _taskCommentService;
        
        public TaskCommentsController(
            IUsersProxyClient usersClient,
            ILogger<BaseController> logger,
            ITaskCommentService taskCommentService)
            : base(logger, usersClient)
        {
            _taskCommentService = taskCommentService;
        }

        [HttpPost]
        public async Task<ActionResult<TaskModel>> Create([FromForm] TaskCommentCreateRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.LegalInitiator, Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            await _taskCommentService.CreateTaskComment(request, authInfo);
            return Ok();
        }
    }
}