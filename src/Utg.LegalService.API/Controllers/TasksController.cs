using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TasksController : BaseController
    {
        private readonly ITaskService taskService;
        
        public TasksController(
            IUsersProxyClient usersClient,
            ILogger<BaseController> logger,
            ITaskService taskService)
            : base(logger, usersClient)
        {
            this.taskService = taskService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TaskModel>>> Get([FromQuery]TaskRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await taskService.GetAll(request, authInfo);
            return result;
        }
        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskModel>> GetById(int id)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await taskService.GetById(id, authInfo);
            return Ok(result);
        }
        
        [HttpPost]
        public async Task<ActionResult<TaskModel>> Create([FromForm] TaskCreateRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await taskService.CreateTask(request, authInfo);
            return this.Ok(result);
        }

        [HttpPatch]
        public async Task<ActionResult<TaskModel>> Update([FromForm] TaskUpdateRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await taskService.UpdateTask(request, authInfo);
            return Ok(result);
        }
        
        [HttpPatch("inwork")]
        public async Task<ActionResult<TaskModel>> UpdateMoveToInWork([FromForm] TaskUpdateMoveToInWorkRequest request)
        {
            if (!await CanGo(Role.LegalHead))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await taskService.UpdateTaskMoveToInWork(request, authInfo);
            return Ok(result);
        }
        
        [HttpPatch("underreview")]
        public async Task<ActionResult<TaskModel>> UpdateMoveToUnderReview([FromForm] TaskUpdateMoveToUnderReviewRequest request)
        {
            if (!await CanGo(Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await taskService.UpdateTaskMoveToUnderReview(request, authInfo);
            return Ok(result);
        }
        
        [HttpPatch("done")]
        public async Task<ActionResult<TaskModel>> UpdateMoveToDone([FromForm] TaskUpdateMoveToDoneRequest request)
        {
            if (!await CanGo(Role.LegalHead))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await taskService.UpdateTaskMoveToDone(request, authInfo);
            return Ok(result);
        }
        
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (!await CanGo(Role.IntranetUser))
            {
                return Forbid();
            }
            await taskService.DeleteTask(id);
            return Ok();
        }
        
        [HttpGet("report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<File>> GetReport([FromQuery]TaskReportRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var stream = await taskService.GetReport(request, authInfo);
            return this.File(stream, MediaTypeNames.Application.Octet, "Отчет о задачах.xlsx");
        }
    }
}