using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;
using System.Text.Json;
using DungeonCrawler_Game_Service.Application;
using DungeonCrawler_Game_Service.Application.Behavior;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Application.Features.Characters.Validators;
using DungeonCrawler_Game_Service.Models;
using DungeonCrawlerAssembly.Messages;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using DungeonCrawler_Game_Service.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION INFRASTRUCTURE ---

// Sans ça, les redirects HTTPS échouent et cassent le CORS.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // On vide les réseaux connus et proxy connus pour accepter tout (en dev/staging c'est ok)
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
    Console.WriteLine($"CRITICAL ERROR: MongoDb Configuration Failed: {ex.Message}");
    // On ne crash pas ici pour laisser les logs apparaitre, mais l'app sera instable
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

// --- 2. CONFIGURATION APPLICATION ---

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
    
    // Mappings Swagger (ErrorResponse, EventSchema, etc.) conservés...
    options.MapType<ErrorResponse>(() => new OpenApiSchema { Type = "object", Properties = { ["code"] = new OpenApiSchema { Type = "string" }, ["message"] = new OpenApiSchema { Type = "string" } } });
});

// On utilise SetIsOriginAllowed qui est plus souple que WithOrigins pour le dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true) // Accepte TOUTES les origines dynamiquement tout en autorisant les credentials
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
        
        // Debug pour voir si le token est rejeté
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication Failed: {context.Exception.Message}");
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
        .Routing(r => r.TypeBased().MapAssemblyOf<ApplicationAssemblyReference>("rabbitmq-queue"))
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

// --- 3. PIPELINE MIDDLEWARE (ORDRE CRITIQUE) ---

// 1. Forwarded Headers (Doit être tout en haut pour gérer le Proxy OVH)
app.UseForwardedHeaders(); 


// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DungeonCrawler Game Service v1"));
}

// 2. Gestion des exceptions globale (pour éviter que l'app crash sans headers CORS)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        // On force les headers CORS même en cas d'erreur 500
        context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "*");
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"Internal Server Error (Check Logs)\"}");
    });
});

// 3. HttpsRedirection (Peut être désactivé si le Proxy gère déjà le HTTPS, mais gardons-le avec ForwardedHeaders)
app.UseHttpsRedirection();

// 4. CORS
app.UseCors("AllowAll");

// 5. Auth
app.UseAuthentication();
app.UseAuthorization();

// 6. Validation Error Middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (FluentValidation.ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        await context.Response.WriteAsJsonAsync(new { Errors = errors });
    }
});

app.MapControllers();

app.MapGet("/health", () => Results.Ok("Service is UP")); // Endpoint de test simple sans Auth

await app.RunAsync();