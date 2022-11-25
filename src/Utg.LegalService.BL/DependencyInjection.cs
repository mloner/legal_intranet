using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utg.Common.MediatR;
using Utg.LegalService.Dal;

namespace Utg.LegalService.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
        return services
            .AddDataAccess(configuration, "UTGDatabase")
            .AddUtgMediatr(typeof(DependencyInjection)); 
    }
}
