using EchoCore.Application.Services;
using EchoCore.Domain.Factories;
using EchoCore.Domain.Models.Common;
using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;
using EchoCore.Domain.Utilities;
using EchoCore.Infrastructure.Data;
using EchoCore.Infrastructure.Factories;
using EchoCore.Infrastructure.Repositories;
using EchoCore.Infrastructure.Services;
using EchoCore.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

#region 🔧 Configuration + Options
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<AgentRunnerOptions>(builder.Configuration.GetSection("AgentRunner"));
#endregion

#region 🔧 Core Services
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
#endregion

#region 🔢 API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
#endregion

#region 🔒 Rate Limiting
builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("fixed", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 50;
    }));
#endregion

#region 🗄️ Database
builder.Services.AddDbContext<MemoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHealthChecks().AddDbContextCheck<MemoryDbContext>("Database");
#endregion

#region 🏗️ Dependency Injection
// Core Repositories & Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<ISemanticMemoryRepository, SemanticMemoryRepository>();
builder.Services.AddScoped<IGptClient, GptClient>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IMemorySystemClient, MemorySystemClient>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IThreadService, ThreadService>();
builder.Services.AddScoped<IPromptProfileRepository, PromptProfileRepository>();
builder.Services.AddScoped<IPromptProfileService, PromptProfileService>();
builder.Services.AddScoped(typeof(IAgentRunner), typeof(AgentRunner));
builder.Services.AddScoped(typeof(IHateoasFactory<>), typeof(DynamicHateoasFactory<>));
builder.Services.AddScoped(typeof(IOutputFormatChecker), typeof(OutputFormatChecker));
builder.Services.AddScoped(typeof(IFeedbackPromptBuilder), typeof(FeedbackPromptBuilder));

// Multi-Agent Orchestration
builder.Services.AddScoped<IPlanningAgent, OrchestrationPlanner>();
builder.Services.AddSingleton<IAgentRegistry, DistributedAgentRegistry>();
builder.Services.AddScoped<IMultiAgentOrchestrator, MultiAgentOrchestrator>();
#endregion

#region 🔍 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "EchoCore API", Version = "v1" });
    options.SwaggerDoc("v0", new OpenApiInfo { Title = "EchoCore API (Deprecated)", Version = "v0", Description = "Deprecated endpoints." });
});
#endregion

#region 🌐 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
#endregion

var app = builder.Build();

#region 🔄 Database Migration on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MemoryDbContext>();
    db.Database.Migrate();

    var services = scope.ServiceProvider;
    await DatabaseSeeder.SeedAsync(services);
}
#endregion

#region 🚨 Global Exception Handler
app.UseExceptionHandler(config =>
{
    config.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
        var ex = exceptionHandler?.Error;

        context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
        logger.LogError(ex, "Unhandled exception occurred");

        var response = new
        {
            error = ex?.Message ?? "An unknown error occurred",
            details = builder.Environment.IsDevelopment() ? ex?.StackTrace : null
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});
#endregion

#region 🚀 Middleware Pipeline
var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var desc in apiVersionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
    }
    options.DisplayRequestDuration();
    options.EnableFilter();
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description }),
            totalDuration = report.TotalDuration.TotalSeconds
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});
#endregion

app.Run();
