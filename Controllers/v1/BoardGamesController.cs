using System.Linq.Dynamic.Core;
using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyBGList.Constants;
using MyBGList.DTOs;
using MyBGList.DTOs.v1;
using MyBGList.Models;

namespace MyBGList.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class BoardGamesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BoardGamesController> _logger;
    private readonly IMemoryCache _memoryCache;

    public BoardGamesController(ILogger<BoardGamesController> logger, ApplicationDbContext context,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _context = context;
        _memoryCache = memoryCache;
    }

    [HttpGet(Name = "GetBoardGames")]
    [ResponseCache(CacheProfileName = "Any-60")]
    public async Task<DTOs.v1.RestDTO<BoardGame[]>> Get([FromQuery] RequestDTO<BoardGameDTO> input)
    {
        _logger.LogInformation(CustomLogEvents.BoardGamesController_Get,
            "Get method started [{MachineName}] [{ThreadId}]", Environment.MachineName,
            Environment.CurrentManagedThreadId);

        var query = _context.BoardGames!.AsQueryable();

        if (!string.IsNullOrWhiteSpace(input.FilterQuery)) query = query.Where(b => b.Name.Contains(input.FilterQuery));

        var recordCount = await query.CountAsync();

        BoardGame[]? result = null;
        var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";
        if (!_memoryCache.TryGetValue<BoardGame[]>(cacheKey, out result))
        {
            query = query.OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);

            result = await query.ToArrayAsync();

            _memoryCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
        }

        return new DTOs.v1.RestDTO<BoardGame[]>
        {
            Data = result ?? Array.Empty<BoardGame>(),
            PageIndex = input.PageIndex,
            PageSize = input.PageSize,
            RecordCount = recordCount,
            Links = new List<LinkDTO>
            {
                new(
                    Url.Action(null, "BoardGames",
                        new { input.PageIndex, input.PageSize },
                        null,
                        Request.Scheme)!,
                    "self",
                    "GET")
            }
        };
    }

    [HttpPost(Name = "UpdateBoardGame")]
    [ResponseCache(NoStore = true)]
    public async Task<DTOs.v1.RestDTO<BoardGame?>> Post(BoardGameDTO model)
    {
        var boardgame = await _context.BoardGames!
            .Where(b => b.Id == model.Id)
            .FirstOrDefaultAsync();

        if (boardgame != null)
        {
            if (!string.IsNullOrEmpty(model.Name))
                boardgame.Name = model.Name;

            if (model.Year.HasValue && model.Year.Value > 0)
                boardgame.Year = model.Year.Value;

            boardgame.LastModifiedDate = DateTime.Now;

            _context.BoardGames!.Update(boardgame);

            await _context.SaveChangesAsync();
        }

        return new DTOs.v1.RestDTO<BoardGame?>
        {
            Data = boardgame,
            Links = new List<LinkDTO>
            {
                new(
                    Url.Action(
                        null,
                        "BoardGames",
                        model,
                        Request.Scheme)!,
                    "self",
                    "POST")
            }
        };
    }

    [HttpDelete(Name = "DeleteBoardGame")]
    [ResponseCache(NoStore = true)]
    public async Task<DTOs.v1.RestDTO<BoardGame?>> Delete(int id)
    {
        var boardgame = await _context.BoardGames!
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (boardgame != null)
        {
            _context.BoardGames!.Remove(boardgame);

            await _context.SaveChangesAsync();
        }

        return new DTOs.v1.RestDTO<BoardGame?>
        {
            Data = boardgame,
            Links = new List<LinkDTO>
            {
                new(
                    Url.Action(
                        null,
                        "BoardGames",
                        id,
                        Request.Scheme)!,
                    "self",
                    "DELETE")
            }
        };
    }
}