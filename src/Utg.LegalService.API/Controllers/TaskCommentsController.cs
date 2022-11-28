using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request.TaskComments;
using Utg.LegalService.Common.Services;
using Utg.Common.Packages.Domain.Enums;

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
        public async Task<ActionResult<List<TaskCommentModel>>> Create([FromForm] TaskCommentCreateRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            await _taskCommentService.CreateTaskComment(request, authInfo);
            var result = await GetByTaskId(request.TaskId);
            return result;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<TaskCommentModel>>> GetByTaskId(int taskId)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var comments = await _taskCommentService.GetByTaskId(taskId);
            return Ok(comments);
        }
    }
}