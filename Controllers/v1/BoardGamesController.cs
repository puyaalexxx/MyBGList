using System.Linq.Dynamic.Core;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<RestDTO<BoardGame[]>> Get(int pageIndex = 0, int pageSize = 10, string? sortColumn = "Name",
        string? sortOrder = "ASC", string? filterQuery = null)
    {
        var query = _context.BoardGames!.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterQuery)) query = query.Where(b => b.Name.Contains(filterQuery));

        var recordCount = await query.CountAsync();

        query = query.OrderBy($"{sortColumn} {sortOrder}")
            .Skip(pageIndex * pageSize)
            .Take(pageSize);

        return new RestDTO<BoardGame[]>
        {
            /*Data = new BoardGame[] {
                new BoardGame() {
                    Id = 1,
                    Name = "Axis & Allies",
                    Year = 1981
                },
                new BoardGame() {
                    Id = 2,
                    Name = "Citadels",
                    Year = 2000
                },
                new BoardGame() {
                    Id = 3,
                    Name = "Terraforming Mars",
                    Year = 2016
                }
            },*/
            Data = await query.ToArrayAsync(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            RecordCount = recordCount,
            Links = new List<LinkDTO>
            {
                new(
                    Url.Action(null, "BoardGames", new { pageIndex, pageSize }, null, Request.Scheme)!,
                    "self",
                    "GET")
            }
        };
    }

    [HttpPost(Name = "UpdateBoardGame")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<BoardGame?>> Post(BoardGameDTO model)
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

        return new RestDTO<BoardGame?>
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
    public async Task<RestDTO<BoardGame?>> Delete(int id)
    {
        var boardgame = await _context.BoardGames!
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync();
        
        if (boardgame != null)
        {
            _context.BoardGames!.Remove(boardgame);
            
            await _context.SaveChangesAsync();
        }
        
        return new RestDTO<BoardGame?>()
        {
            Data = boardgame,
            Links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(
                        null,
                        "BoardGames",
                        id,
                        Request.Scheme)!,
                    "self",
                    "DELETE"),
            }
        };
    }
}