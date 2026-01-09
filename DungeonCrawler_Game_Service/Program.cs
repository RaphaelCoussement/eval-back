using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using DungeonCrawler_Game_Service.Application;
using DungeonCrawler_Game_Service.Application.Behavior;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Application.Features.Characters.Validators;
using DungeonCrawler_Game_Service.Models;
using DungeonCrawlerAssembly.Messages;
using FluentValidation;
using MediatR;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using DungeonCrawler_Game_Service.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURATION OPENTELEMETRY (OBSERVABILITY)
// =========================================================================

// A. Configuration des LOGS (capture ILogger)
builder.Logging.ClearProviders(); // Optionnel : nettoie les providers par défaut si tu ne veux que OTLP + Console
builder.Logging.AddConsole();     // Garde la console pour le dev local
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter(); // Lit automatiquement OTEL_EXPORTER_OTLP_ENDPOINT & OTEL_LOGS_EXPORTER
});

// B. Configuration des TRACES et METRICS
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName)) // Nom par défaut, écrasé par OTEL_SERVICE_NAME
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation() // Trace les Controllers/API
            .AddHttpClientInstrumentation() // Trace les appels sortants (ex: vers Keycloak ou autre API)
            .AddOtlpExporter();             // Envoie vers le collector Ops
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });

// =========================================================================
// 2. CONFIGURATION INFRASTRUCTURE
// =========================================================================

// Sans ça, les redirects HTTPS échouent et cassent le CORS.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configuration MongoDB
try 
{
    MongoDbConfiguration.Configure();
}
catch(Exception ex)
{
    // Ici, on utilise Console.WriteLine car le logger n'est pas encore totalement construit, 
    // ou on pourrait récupérer un logger temporaire, mais Console est OK ici.
    Console.WriteLine($"CRITICAL ERROR: MongoDb Configuration Failed: {ex.Message}");
}

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// =========================================================================
// 3. CONFIGURATION APPLICATION
// =========================================================================

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateCharacterCommandHandler>());

builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DungeonCrawler Game Service API", Version = "v1" });
    options.AddSecurityDefinition(AuthSchemes.Bearer, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = AuthSchemes.Bearer,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = AuthSchemes.Bearer }
            },
            Array.Empty<string>()
        }
    });
    
    options.MapType<ErrorResponse>(() => new OpenApiSchema { Type = "object", Properties = { ["code"] = new OpenApiSchema { Type = "string" }, ["message"] = new OpenApiSchema { Type = "string" } } });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(AuthSchemes.Bearer)
    .AddJwtBearer(AuthSchemes.Bearer, options =>
    {
        options.Authority = builder.Configuration["KeycloakSettings:Authority"];
        options.Audience = builder.Configuration["KeycloakSettings:Audience"];
        options.RequireHttpsMetadata = false; 
        
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Modification ici : utiliser le Logger du context plutôt que Console.WriteLine
                var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                logger?.LogError(context.Exception, "Authentication Failed");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ApiUser", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("preferred_username");
    });

// Rebus
builder.Services.AddRebus((configure, sp) =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var database = client.GetDatabase(settings.DatabaseName);

    return configure
        .Logging(l => l.MicrosoftExtensionsLogging(sp.GetRequiredService<ILoggerFactory>())) // IMPORTANT : Connecter Rebus aux logs .NET
        .Routing(r => r.TypeBased().MapAssemblyOf<ApplicationAssemblyReference>("rabbitmq-queue"))
        .Routing(r => r.TypeBased()
            .MapAssemblyOf<ApplicationAssemblyReference>("rabbitmq-queue")
            .MapAssemblyOf<CharacterCreationConfirmed>("rabbitmq-queue")) // Mappe aussi les types du package externe
        .Transport(t => t.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMq"), "rabbitmq-queue"))
        .Sagas(s => s.StoreInMongoDb(database, type => "RebusSagas", true));
},
    onCreated: async bus =>
    {
        await bus.Subscribe<CharacterCreationConfirmed>();
        await bus.Subscribe<CharacterCreationFailed>();
    })
    .AutoRegisterHandlersFromAssemblyOf<ApplicationAssemblyReference>();


var app = builder.Build();

// =========================================================================
// 4. PIPELINE MIDDLEWARE
// =========================================================================

app.UseForwardedHeaders(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DungeonCrawler Game Service v1"));
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        // On log l'erreur ici aussi via ILogger
        var logger = context.RequestServices.GetService<ILogger<Program>>();
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        
        if (exceptionHandlerPathFeature?.Error != null)
        {
            logger?.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred.");
        }

        context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "*");
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"Internal Server Error (Check Logs)\"}");
    });
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (FluentValidation.ValidationException ex)
    {
        // Tu peux logger les erreurs de validation en Warning si tu veux surveiller la qualité des données entrantes
        var logger = context.RequestServices.GetService<ILogger<Program>>();
        logger?.LogWarning("Validation failed: {ValidationErrors}", ex.Message);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        await context.Response.WriteAsJsonAsync(new { Errors = errors });
    }
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok("Service is UP"));

await app.RunAsync();