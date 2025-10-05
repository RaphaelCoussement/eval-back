using DungeonCrawler_Game_Service.Domain.Entities;
using MediatR;

namespace DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;

public class GenerateDungeonCommand : IRequest<Dungeon>;