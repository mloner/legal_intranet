using System.Net.Mime;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Models.Client;
using Utg.Common.Packages.Domain.Models.Enum;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.BL.Features.SubTask.Create;
using Utg.LegalService.Common.Models.Client;
using Utg.LegalService.Common.Models.Client.Task;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TasksController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly IMediator _mediator;
        
        public TasksController(
            IUsersProxyClient usersClient,
            ILogger<BaseController> logger,
            ITaskService taskService,
            IMediator mediator)
            : base(logger, usersClient)
        {
            this._taskService = taskService;
            _mediator = mediator;
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
            var result = await _taskService.GetAll(request, authInfo);
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
            var result = await _taskService.GetById(id, authInfo);
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
            var result = await _taskService.CreateTask(request, authInfo);
            return Ok(result);
        }
        
        [HttpPost("subtask")]
        public async Task<ActionResult<TaskModel>> CreateSubtask(
            [FromForm] SubtaskCreateRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.LegalPerformer))
            {
                return Forbid();
            }

            var authInfo = await GetAuthInfo();
            var command = request.Adapt<CreateSubtaskCommand>();
            command.AuthInfo = authInfo;
            var response = await _mediator.Send(command, HttpContext.RequestAborted);

            return response.Success ? Ok(response.Data) : StatusCode(response.StatusCode, response.Message);
        }

        [HttpPatch("inwork")]
        public async Task<ActionResult<TaskModel>> UpdateMoveToInWork([FromForm] TaskUpdateMoveToInWorkRequest request)
        {
            if (!await CanGo(Role.LegalHead))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            var result = await _taskService.UpdateTaskMoveToInWork(request, authInfo);
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
            var result = await _taskService.UpdateTaskMoveToUnderReview(request, authInfo);
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
            var result = await _taskService.UpdateTaskMoveToDone(request, authInfo);
            return Ok(result);
        }
        
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (!await CanGo(Role.IntranetUser))
            {
                return Forbid();
            }
            await _taskService.DeleteTask(id);
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
            var stream = await _taskService.GetReport(request, authInfo);
            return this.File(stream, MediaTypeNames.Application.Octet, "Отчет о задачах.xlsx");
        }
    }
}