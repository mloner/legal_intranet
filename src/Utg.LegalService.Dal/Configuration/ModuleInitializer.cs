using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal.Configuration
{
    public static class ModuleInitializer
    {
        public static IServiceCollection ConfigureDal(this IServiceCollection services, IConfiguration configuration)
        {
            SetSettings(services, configuration);
            AddDependenciesToContainer(services);

            return services;
        }

        private static void SetSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UtgContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("UTGDatabase"));
            });
        }

        private static void AddDependenciesToContainer(IServiceCollection services)
        {
        }
    }
}
