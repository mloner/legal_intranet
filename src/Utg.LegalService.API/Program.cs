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
using Utg.Common.Packages.Domain.Filters;
using Utg.Common.Packages.Domain.MiddleWares;
using Utg.Common.Packages.ExcelReportBuilder.Configuration;
using Utg.Common.Packages.FileStorage.Configuration;
using Utg.Common.Packages.Queue.Configuration;
using Utg.Common.Packages.ServiceClientProxy.AuthHeader;
using Utg.Common.Packages.ServiceClientProxy.Configuration;
using Utg.LegalService.API.Configuration;
using Utg.LegalService.API.Middlewares;
using Utg.LegalService.BL;
using Utg.LegalService.Jobs;
using Utg.LegalService.Jobs.NotifyExpiredSoonTasksJob;
using Utg.LegalService.Jobs.UpdateJobs.UpdateCompanyHostedService;
using Utg.LegalService.Jobs.UpdateJobs.UpdateDepartmentHostedService;
using Utg.LegalService.Jobs.UpdateJobs.UpdatePositionHostedService;
using Utg.LegalService.Jobs.UpdateJobs.UpdateUserProfileHostedService;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddFluentValidation()
    .AddNewtonsoftJson(
        options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });
AddLogging(builder);

builder.Services
    .AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

IdentityModelEventSource.ShowPII = true; 

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.Authority = builder.Configuration["KeyCloak:Authority"];
    o.Audience = builder.Configuration["KeyCloak:Audience"];
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

string _corsPolicy = "corsPolicy";
builder.Services.AddCors(options =>
    options.AddPolicy(
        name: _corsPolicy,
        builder =>
        {
            builder
                .WithOrigins("http://localhost:8080")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));

var minioConfiguration = builder.Configuration.GetSection("Minio").Get<MinioFileStorageSettings>();
var config = new MapperConfiguration(cfg => AutoMapperConfig.ConfigureMappings(cfg));
config.AssertConfigurationIsValid();
builder.Services.AddSingleton(config);
builder.Services.AddSingleton(config.CreateMapper());
builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("UTGDatabase")));
builder.Services.AddSwaggerDocument(opts => opts.Title = "Legal service Api");
builder.Services.ConfigureMinioFileStorage(minioConfiguration);
builder.Services.AddBusiness(builder.Configuration);

builder.Services
    .ConfigureExcelReportBuilder()
    .ConfigureServiceClientProxy(
        new ServiceClientProxySettings(
            builder.Configuration.GetValue<string>("Api:Main"),
            builder.Configuration.GetValue<string>("Api:Task")),
        builder.Configuration.GetSection("BasicAuth").Get<BasicAuthConfig>())
    .ConfigureRabbitMq(builder.Configuration.GetSection("Queue").Get<RabbitMqSettings>());

AddJobs(builder.Services);

static bool DoValidation(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
{
    return true;
}

static void AddJobs(IServiceCollection services)
{
    services.AddHostedService<UpdateCompanyHostedService>();
    services.AddHostedService<UpdateDepartmentHostedService>();
    services.AddHostedService<UpdatePositionHostedService>();
    services.AddHostedService<UpdateUserProfileHostedService>();
    services.AddScoped<NotifyExpiredSoonTasksJob, NotifyExpiredSoonTasksJob>();      
}


static WebApplicationBuilder AddLogging(WebApplicationBuilder builder)
{
    Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

    var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();

    builder.Logging.AddSerilog(logger);

    return builder;
}



var app = builder.Build();

app.UseRequestResponseLogging();
app.UseCors(_corsPolicy);
app.UseHangfireServer();
app.UseHangfireDashboard("/legal/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter(app.Configuration) }
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthHeaderWrapperMiddleware>();
            

app.UseEndpoints(endpoints => endpoints.MapControllers());

if (app.Environment.EnvironmentName.StartsWith("Dev") || app.Environment.IsDevelopment())
{
    var swaggerPath = $"/legal/swagger";
    app.UseOpenApi(options =>
    {
        options.Path = $"{swaggerPath}/v1/swagger.json";
    });

    app.UseSwaggerUi3(options =>
    {
        options.Path = swaggerPath;
        options.DocumentPath = $"/{swaggerPath}/" + "v1" + "/swagger.json";
    });
}
            
ConfigureJobs(app);

app.Run();


static void ConfigureJobs(WebApplication app)
{
    ConfigureJob<NotifyExpiredSoonTasksJob>(app.Configuration);
}
        
static void ConfigureJob<T>(
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

