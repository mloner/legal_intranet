using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal.Interceptors;
using Utg.LegalService.Dal.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal;

/// <summary>
/// Этот класс изолирует настройку уровня доступа к данным от основного проекта api
/// </summary>
public static class DependencyInjection
{
    private const string DefaultSection = "default";
    
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = DefaultSection)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        services.AddDbContext<UtgContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(sectionName));
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.AddInterceptors(new DateTimeStampsInterceptor());
        });

        AddDependenciesToContainer(services);
        using (var serviceScope = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetService<UtgContext>();
            context.Database.Migrate();
        }

        return services;
    }
    
    private static void AddDependenciesToContainer(IServiceCollection services)
    {
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskAttachmentRepository, TaskAttachmentRepository>();
        services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
        services.AddScoped<ITaskChangeHistoryRepository, TaskChangeHistoryRepository>();
        services.AddScoped<IUserProfileAgregateRepository, UserProfileAgregateRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}

