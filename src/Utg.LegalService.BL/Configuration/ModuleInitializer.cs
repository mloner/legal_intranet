using Microsoft.Extensions.DependencyInjection;
using Utg.LegalService.BL.Services;
using Utg.LegalService.Common.Services;

namespace Utg.LegalService.BL.Configuration
{
    public static class ModuleInitializer
    {
        public static IServiceCollection ConfigureBL(this IServiceCollection services )
        {
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<ITaskCommentService, TaskCommentService>();
            services.AddTransient<INotificationService, NotificationService>();
            
            return services;
        }
    }
}
