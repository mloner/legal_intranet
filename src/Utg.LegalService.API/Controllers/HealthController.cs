using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utg.Common.Packages.ServiceClientProxy.Proxy;

namespace Utg.LegalService.API.Controllers
{
	[Route("legal/[controller]")]
	public class HealthController : BaseController
	{
		public HealthController(
			ILogger<BaseController> logger,
			IUsersProxyClient userClient)
			: base(logger, userClient)
		{
			
		}

		[HttpGet("ping")]
		public IActionResult Ping()
		{
			return this.Ok("legal");
		}
	}
}
