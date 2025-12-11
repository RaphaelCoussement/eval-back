
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
using Rebus.Config;
using DungeonCrawler_Game_Service.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuration MongoDB AVANT toute utilisation de MongoDB ou Rebus
MongoDbConfiguration.Configure();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// MongoClient singleton
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// MongoDatabase scoped
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// UnitOfWork scoped
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();



// Ajout de MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateCharacterCommandHandler>());

// Ajout de FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration enrichie
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DungeonCrawler Game Service API",
        Version = "v1",
        Description = "API for managing dungeons, characters, and game logic",
        Contact = new OpenApiContact
        {
            Name = "DungeonCrawler Team",
            Email = "support@dungeoncrawler.com"
        }
    });
    // Auth JWT dans Swagger
    options.AddSecurityDefinition(AuthSchemes.Bearer, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = AuthSchemes.Bearer,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez 'Bearer {votre_token}' pour vous authentifier"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = AuthSchemes.Bearer
                }
            },
            Array.Empty<string>()
        }
    });

    // Inclure les commentaires XML si tu veux des descriptions plus précises
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Définition des schémas globaux
    options.MapType<ErrorResponse>(() => new OpenApiSchema
    {
        Type = "object",
        Properties =
        {
            ["code"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("ROOM_NOT_FOUND") },
            ["message"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("The specified room does not exist") }
        },
        Required = new HashSet<string> { "code", "message" }
    });

    options.MapType<EventSchema>(() => new OpenApiSchema
    {
        Type = "object",
        Properties =
        {
            ["eventId"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("uuid-12345") },
            ["eventVersion"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("v1") },
            ["eventType"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("DungeonGenerated") },
            ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new Microsoft.OpenApi.Any.OpenApiString("2025-10-01T10:00:00Z") },
            ["data"] = new OpenApiSchema { Type = "object" }
        },
        Required = new HashSet<string> { "eventId", "eventVersion", "eventType", "timestamp" }
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configuration de l'authentification JWT avec Keycloak
builder.Services.AddAuthentication(AuthSchemes.Bearer)
    .AddJwtBearer(AuthSchemes.Bearer, options =>
    {
        options.Authority = builder.Configuration["KeycloakSettings:Authority"];
        options.Audience = builder.Configuration["KeycloakSettings:Audience"];
        options.RequireHttpsMetadata = true; // à true en production
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ApiUser", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("preferred_username");
    });

builder.Services.AddRebus((configure, sp) =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var database = client.GetDatabase(settings.DatabaseName);
    

    return configure
        .Routing(r =>
            r.TypeBased().MapAssemblyOf<ApplicationAssemblyReference>("rabbitmq-queue"))
        .Transport(t =>
            t.UseRabbitMq(
                builder.Configuration.GetConnectionString("RabbitMq"),
                "rabbitmq-queue"))
        .Sagas(s =>
            s.StoreInMongoDb(
                database,
                type => "RebusSagas",
                true));
},
    onCreated: async bus =>
    {
        // S'abonne aux messages de réponse des autres microservices
        await bus.Subscribe<CharacterCreationConfirmed>();
        await bus.Subscribe<CharacterCreationFailed>();
    })
    // Enregistre automatiquement tous les handlers (y compris les sagas) de l'assembly Application
    .AutoRegisterHandlersFromAssemblyOf<ApplicationAssemblyReference>();


var app = builder.Build();
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DungeonCrawler Game Service v1");
    });
}

// Permet de renvoyer une 400 bad request pour les erreur de validation FluentValidtion
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


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/secure", [Authorize(Policy = "ApiUser")] () =>
{
    return Results.Ok("Accès autorisé !");
});

await app.RunAsync();