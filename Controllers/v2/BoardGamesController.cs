using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBGList.DTOs.v2;
using MyBGList.Models;

namespace MyBGList.Controllers.v2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BoardGamesController : ControllerBase
    {
        private readonly ILogger<BoardGamesController> _logger;

        public BoardGamesController(ILogger<BoardGamesController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetBoardGames")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public RestDTO<BoardGame[]> Get()
        {
            return new RestDTO<BoardGame[]>()
            {
                Items = new BoardGame[] {
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
                },
                Links = new List<LinkDTO> {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", null, Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }
    }
}
