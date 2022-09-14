using Microsoft.Extensions.DependencyInjection;

namespace Utg.LegalService.BL.Configuration
{
    public static class ModuleInitializer
    {
        public static IServiceCollection ConfigureBL(this IServiceCollection services )
        {
            return services;
        }
    }
}
