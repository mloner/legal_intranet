using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.Common.EF.Repositories;
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
        });

        services.AddScoped<UnitOfWork>();
        using (var serviceScope = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetService<UtgContext>();
            context.Database.Migrate();
        }

        return services;
    }
}

