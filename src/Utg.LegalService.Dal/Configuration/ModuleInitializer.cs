using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.Interceptors;
using Utg.LegalService.Dal.Repositories;
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
            services.AddDbContextPool<UtgContext>(options =>
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                options.UseNpgsql(configuration.GetConnectionString("UTGDatabase"));
                options.AddInterceptors(new DateTimeStampsInterceptor());
            });
        }

        private static void AddDependenciesToContainer(IServiceCollection services)
        {
            services.AddTransient<ITaskRepository, TaskRepository>();
            services.AddTransient<ITaskAttachmentRepository, TaskAttachmentRepository>();
            services.AddTransient<ITaskCommentRepository, TaskCommentRepository>();
            services.AddTransient<IAgregateRepository, AgregateRepository>();
        }
    }
}
