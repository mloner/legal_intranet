using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.Common.MediatR;
using Utg.LegalService.BL.Services;
using Utg.LegalService.Common.Services;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
        
        services.AddTransient<ITaskService, TaskService>();
        services.AddTransient<ITaskCommentService, TaskCommentService>();
        services.AddTransient<INotificationService, NotificationService>();

        services.AddDataAccess(configuration, "UTGDatabase");
        services.AddUtgMediatr(typeof(DependencyInjection)); 
        
        return services;
    }
}
