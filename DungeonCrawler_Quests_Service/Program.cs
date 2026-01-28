using DungeonCrawler_Quests_Service.Infrastructure.Interfaces;
using DungeonCrawler_Quests_Service.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using DungeonCrawler_Quests_Service.Application;
using DungeonCrawler_Quests_Service.Application.Behavior;
using DungeonCrawler_Quests_Service.Datas;
using FluentValidation;
using MediatR;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using DungeonCrawler_Quests_Service.Infrastructure;
using DungeonCrawler_Quests_Service.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using DungeonCrawlerAssembly.Messages;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURATION OPENTELEMETRY
// =========================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "QuestsService"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddOtlpExporter();
    });

// =========================================================================
// 2. CONFIGURATION INFRASTRUCTURE (MongoDB)
// =========================================================================
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

MongoDbConfiguration.Configure();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// =========================================================================
// 3. CONFIGURATION APPLICATION (MediatR & Rebus)
// =========================================================================

// On utilise ApplicationAssemblyReference pour scanner automatiquement tout le projet Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyReference>());

builder.Services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyReference>();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyReference>();
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DungeonCrawler Quests Service API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

// Rebus (Messagerie)
builder.Services.AddRebus((configure, sp) =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var database = client.GetDatabase(settings.DatabaseName);

    return configure
        .Logging(l => l.MicrosoftExtensionsLogging(sp.GetRequiredService<ILoggerFactory>()))
        .Routing(r => r.TypeBased()
            .MapAssemblyOf<DungeonCompleted>("quests-queue") // On écoute les events donjons
        )
        .Transport(t => t.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMq"), "quests-queue"))
        .Sagas(s => s.StoreInMongoDb(database, type => "RebusSagas", true));
},
onCreated: async bus =>
{
    // C'est ici que tu t'abonnes à l'événement du service Game
    await bus.Subscribe<DungeonCompleted>(); 
})
.AutoRegisterHandlersFromAssemblyOf<ApplicationAssemblyReference>();

var app = builder.Build();

// =========================================================================
// 4. PIPELINE MIDDLEWARE
// =========================================================================
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseForwardedHeaders(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DungeonCrawler Quests Service v1"));
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("Quests Service is UP"));

await app.RunAsync();