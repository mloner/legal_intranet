using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Utg.LegalService.Jobs
{
    public class BaseJob
    {
        protected readonly ILogger<BaseJob> _logger;
        protected readonly IConfiguration _configuration;

        protected string JobName { get; set; }

        public BaseJob(
            ILogger<BaseJob> logger,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _configuration = configuration;
        }

    }
}