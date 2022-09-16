using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Utg.Common.Packages.Domain.MiddleWares;
using Utg.Common.Packages.Queue.Configuration;
using Utg.Common.Packages.ServiceClientProxy.AuthHeader;
using Utg.Common.Packages.ServiceClientProxy.Configuration;
using Utg.LegalService.API.Configuration;
using Utg.LegalService.API.Middlewares;
using Utg.LegalService.BL.Configuration;
using Utg.LegalService.Dal.Configuration;

namespace Utg.LegalService.API
{
    public class Startup
    {

        private readonly IConfiguration configuration;
        private readonly ILogger<Startup> _logger;
        private readonly string _corsPolicy = "corsPolicy";
        public Startup(IConfiguration configuration,
            ILogger<Startup> logger)
        {
            this.configuration = configuration;
            _logger = logger;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddFluentValidation()
                .AddNewtonsoftJson(
                 options =>
                 {
                     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                 });
            services
                .AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            IdentityModelEventSource.ShowPII = true; //Add this line
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.Authority = configuration["KeyCloak:Authority"];
                o.Audience = configuration["KeyCloak:Audience"];
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters { AudienceValidator = DoValidation };
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });


            services.AddCors(options =>
                options.AddPolicy(
                    name: _corsPolicy,
                    builder =>
                    {
                        builder
                        .WithOrigins("http://localhost:8080")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    }));

            var config = new MapperConfiguration(cfg => AutoMapperConfig.ConfigureMappings(cfg));
            config.AssertConfigurationIsValid();
            services.AddSingleton(config);
            services.AddSingleton(config.CreateMapper());
            services.AddHangfire(x => x.UsePostgreSqlStorage(configuration.GetConnectionString("UTGDatabase")));
            services.AddSwaggerDocument(opts => opts.Title = "Legal service Api");
            services.ConfigureDal(configuration);
            services.ConfigureBL();



            services
                .ConfigureRabbitMq(configuration.GetSection("Queue").Get<RabbitMqSettings>());

            services
                .ConfigureServiceClientProxy(
                    new ServiceClientProxySettings(
                       this.configuration.GetValue<string>("Api:Main"),
                       this.configuration.GetValue<string>("Api:Task")),
                       this.configuration.GetSection("BasicAuth").Get<BasicAuthConfig>())
                .ConfigureRabbitMq(configuration.GetSection("Queue").Get<RabbitMqSettings>());


        }

        public void Configure(IApplicationBuilder builder, IWebHostEnvironment env)
        {
            builder.UseRequestResponseLogging();
            builder.UseCors(_corsPolicy);
            builder.UseHangfireServer();
            builder.UseHangfireDashboard("/legal/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter(configuration) }
            });

            if (env.IsDevelopment())
            {
                builder.UseDeveloperExceptionPage();
            }
            builder.UseRouting();
            builder.UseAuthentication();
            builder.UseAuthorization();
            builder.UseMiddleware<ErrorHandlingMiddleware>();
            builder.UseMiddleware<AuthHeaderWrapperMiddleware>();
            

            builder.UseEndpoints(endpoints => endpoints.MapControllers());

            if (env.EnvironmentName.StartsWith("Dev") || env.IsDevelopment())
            {
                var swaggerPath = $"/legal/swagger";
                builder.UseOpenApi(options =>
                {
                    options.Path = $"{swaggerPath}/v1/swagger.json";
                });

                builder.UseSwaggerUi3(options =>
                {
                    options.Path = swaggerPath;
                    options.DocumentPath = $"/{swaggerPath}/" + "v1" + "/swagger.json";
                });
            }

            Dal.Configuration.Startup.Initialize(builder);
        }

        private bool DoValidation(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            return true;
        }
    }
}
