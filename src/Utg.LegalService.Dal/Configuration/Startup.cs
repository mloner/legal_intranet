using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Utg.LegalService.Dal.SqlContext;

namespace Utg.LegalService.Dal.Configuration
{
    public static class Startup
    {
        public static void Initialize(IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<UtgContext>();
            context.Database.Migrate();
        }
    }
}
