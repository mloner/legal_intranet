using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Utg.Common.Packages.Domain.MiddleWares;
using Utg.Common.Packages.ExcelReportBuilder.Configuration;
using Utg.Common.Packages.FileStorage.Configuration;
using Utg.Common.Packages.Queue.Configuration;
using Utg.Common.Packages.ServiceClientProxy.AuthHeader;
using Utg.Common.Packages.ServiceClientProxy.Configuration;
using Utg.LegalService.API.Configuration;
using Utg.LegalService.API.Middlewares;
using Utg.LegalService.BL;
using Utg.LegalService.Jobs.UpdateJobs.UpdateCompanyHostedService;
using Utg.LegalService.Jobs.UpdateJobs.UpdateDepartmentHostedService;
using Utg.LegalService.Jobs.UpdateJobs.UpdatePositionHostedService;
using Utg.LegalService.Jobs.UpdateJobs.UpdateUserProfileHostedService;
using Utg.LegalService.Jobs;
using Utg.LegalService.Jobs.NotifyExpiredSoonTasksJob;
using Utg.Common.Packages.Domain.Filters;

namespace Utg.LegalService.API
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly string _corsPolicy = "corsPolicy";
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AddLogging(configuration);
            
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

            var minioConfiguration = configuration.GetSection("Minio").Get<MinioFileStorageSettings>();
            var config = new MapperConfiguration(cfg => AutoMapperConfig.ConfigureMappings(cfg));
            config.AssertConfigurationIsValid();
            services.AddSingleton(config);
            services.AddSingleton(config.CreateMapper());
            services.AddHangfire(x => x.UsePostgreSqlStorage(configuration.GetConnectionString("UTGDatabase")));
            services.AddSwaggerDocument(opts => opts.Title = "Legal service Api");
            services.ConfigureMinioFileStorage(minioConfiguration);
            services.AddBusiness(configuration);

            services
                .ConfigureExcelReportBuilder()
                .ConfigureServiceClientProxy(
                    new ServiceClientProxySettings(
                       this.configuration.GetValue<string>("Api:Main"),
                       this.configuration.GetValue<string>("Api:Task")),
                       this.configuration.GetSection("BasicAuth").Get<BasicAuthConfig>())
                .ConfigureRabbitMq(configuration.GetSection("Queue").Get<RabbitMqSettings>());
            
            AddJobs(services);
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
            
            ConfigureJobs();
        }

        private bool DoValidation(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            return true;
        }
        
        private void AddJobs(IServiceCollection services)
        {
            services.AddHostedService<UpdateCompanyHostedService>();
            services.AddHostedService<UpdateDepartmentHostedService>();
            services.AddHostedService<UpdatePositionHostedService>();
            services.AddHostedService<UpdateUserProfileHostedService>();
            services.AddScoped<NotifyExpiredSoonTasksJob, NotifyExpiredSoonTasksJob>();      
        }
        
        private void ConfigureJobs()
        {
            ConfigureJob<NotifyExpiredSoonTasksJob>(configuration);
        }
        
        private static void ConfigureJob<T>(
            IConfiguration configuration,
            string timeTable = "",
            string queueName = "default")
            where T : BaseJob
        {
            var timetable = configuration[$"Jobs:{typeof(T).Name}:Timetable"];
            if (!string.IsNullOrWhiteSpace(timetable))
            {
                RecurringJob.AddOrUpdate<T>(nameof(T), x => x.Start(), timetable, queue: queueName);
            }
        }
        
        public static void AddLogging(IConfiguration configuration)
        {
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}
