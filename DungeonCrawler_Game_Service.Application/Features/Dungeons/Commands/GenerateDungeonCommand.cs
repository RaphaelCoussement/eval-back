using DungeonCrawler_Game_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;

/// <summary>
/// Command pour générer un nouveau donjon.
/// </summary>
public class GenerateDungeonCommand : IRequest<Dungeon>;