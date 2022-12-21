using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.Api.CurrentUserService.Client;
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
        
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITaskCommentService, TaskCommentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IProductionCalendarService, ProductionCalendarService>();

        services.AddDataAccess(configuration, "UTGDatabase");
        services.AddUtgMediatr(typeof(DependencyInjection)); 
        services.AddUtgCurrentUserServiceClient(configuration);
        services.AddUserProfilesService(configuration);
        
        return services;
    }
}
