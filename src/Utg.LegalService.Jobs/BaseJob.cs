using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Utg.LegalService.Jobs
{
    public abstract class BaseJob
    {
        protected readonly ILogger<BaseJob> Logger;
        protected readonly IConfiguration Configuration;

        protected string JobName { get; init; }

        protected BaseJob(
            ILogger<BaseJob> logger,
            IConfiguration configuration
        )
        {
            Logger = logger;
            Configuration = configuration;
        }
        
        public virtual async Task<bool> Start()
        {
            try
            {
                await StartInner();
                Logger.LogInformation($"[Job] \"{JobName}\" finished");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"[Job] \"{JobName}\" failed");
                return false;
            }

            return true;
        }

        protected abstract Task StartInner();

    }
}