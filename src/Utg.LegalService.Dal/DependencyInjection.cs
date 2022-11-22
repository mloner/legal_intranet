using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.Common.EF.Repositories;
using Utg.Common.Extensions;
using Utg.LegalService.Common.Repositories;
using Utg.LegalService.Dal;
using Utg.LegalService.Dal.Repositories;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.ProductionCalendar.DataAccess;

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
        services.AddTransient<ITaskRepository, TaskRepository>();
        services.AddTransient<ITaskAttachmentRepository, TaskAttachmentRepository>();
        services.AddTransient<ITaskCommentRepository, TaskCommentRepository>();
        services.AddTransient<IAgregateRepository, AgregateRepository>();
        services.AddScoped<UnitOfWork>();
    }
}

