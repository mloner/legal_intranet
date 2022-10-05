using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.Domain.Helpers;
using Utg.Common.Packages.ServiceClientProxy.Proxy;
using Utg.LegalService.Common.Services;
using TaskStatus = Utg.LegalService.Common.Models.Client.Enum.TaskStatus;
using TaskType = Utg.LegalService.Common.Models.Client.Enum.TaskType;

namespace Utg.LegalService.API.Controllers
{
    [Route("legal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DictController : BaseController
    {
        private readonly ITaskService taskService;
        public DictController(
            IUsersProxyClient usersClient,
            ILogger<BaseController> logger,
            ITaskService takService)
            : base(logger, usersClient)
        {
            this.taskService = takService;
        }

        [HttpGet("taskStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TaskStatus>> TaskStatus()
        {
            var result = EnumExtensions.GetEnummValuesWithoutDefault<TaskStatus>();
            return Ok(result);
        }

        [HttpGet("taskType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TaskType>> TaskType()
        {
            var result = EnumExtensions.GetEnummValuesWithoutDefault<TaskType>();
            return Ok(result);
        }

        
        [HttpGet("taskAuthorUserProfiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserProfileApiModel>>> TaskAuthorUserProfiles()
        {
            var result = await taskService.GetAuthorUserProfiles();
            return Ok(result);
        }
        
        [HttpGet("performerUserProfiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserProfileApiModel>>> PerformerUserProfiles()
        {
            var result = await taskService.GetPerformerUserProfiles();
            return Ok(result);
        }
    }
}