using System.Linq.Dynamic.Core;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public BoardGamesController(ILogger<BoardGamesController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet(Name = "GetBoardGames")]
    //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
    public async Task<DTOs.v1.RestDTO<BoardGame[]>> Get([FromQuery] RequestDTO<BoardGameDTO> input)
    {
        _logger.LogInformation(CustomLogEvents.BoardGamesController_Get,
            "Get method started [{MachineName}] [{ThreadId}]", Environment.MachineName, Environment.CurrentManagedThreadId);

        var query = _context.BoardGames!.AsQueryable();

        if (!string.IsNullOrWhiteSpace(input.FilterQuery)) query = query.Where(b => b.Name.Contains(input.FilterQuery));

        var recordCount = await query.CountAsync();

        query = query.OrderBy($"{input.SortColumn} {input.SortOrder}")
            .Skip(input.PageIndex * input.PageSize)
            .Take(input.PageSize);

        return new DTOs.v1.RestDTO<BoardGame[]>
        {
            Data = await query.ToArrayAsync(),
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