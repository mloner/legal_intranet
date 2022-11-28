using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Utg.Common.Packages.Domain.Enums;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Models.Request.Tasks;
using Utg.LegalService.Common.Services;
using Utg.Common.Packages.Domain.Enums;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TaskFilesController : BaseController
    {
        private readonly ITaskService _taskService;
        
        public TaskFilesController(
            IUsersProxyClient usersClient,
            ILogger<BaseController> logger,
            ITaskService taskService)
            : base(logger, usersClient)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult> DownloadFile([FromQuery] int attachmentId)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var attachmentModel = await _taskService.DownloadFile(attachmentId);
            return File(
                attachmentModel.Bytes,
                MediaTypeNames.Application.Octet,
                attachmentModel.FileName);
        }
        
        [HttpPost]
        public async Task<ActionResult> UploadFile([FromForm] TaskUploadFileRequest request)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            var authInfo = await GetAuthInfo();
            await _taskService.UploadFile(request, authInfo);
            return this.Ok();
        }
        
        [HttpDelete]
        public async Task<ActionResult> DeleteFile([FromQuery] int attachmentId)
        {
            if (!await CanGo(Role.LegalHead, Role.IntranetUser, Role.LegalPerformer))
            {
                return Forbid();
            }
            await _taskService.DeleteFile(attachmentId);
            return Ok();
        }
    }
}