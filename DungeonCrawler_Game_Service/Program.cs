using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Application.Services;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

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

// DungeonService scoped
builder.Services.AddScoped<IDungeonService, DungeonService>();
// CharacterService scoped
builder.Services.AddScoped<ICharacterService, CharacterService>();

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
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://localhost:8080/realms/katakombs";
        options.RequireHttpsMetadata = false; // à true en production
        options.Audience = "katakombsId"; // audiance configurée dans Keycloak
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiUser", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("preferred_username");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DungeonCrawler Game Service v1");
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/secure", [Authorize(Policy = "ApiUser")] () =>
{
    return Results.Ok("Accès autorisé !");
});

app.Run();

// Classes pour documenter Swagger
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}

public class ErrorResponse
{
    public string Code { get; set; } = null!;
    public string Message { get; set; } = null!;
}

public class EventSchema
{
    public string EventId { get; set; } = null!;
    public string EventVersion { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public object Data { get; set; } = null!;
}