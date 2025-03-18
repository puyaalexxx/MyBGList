using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyBGList.DTOs.v1;
using MyBGList.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace MyBGList.Controllers.v1;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DomainsController> _logger;
    private readonly SignInManager<ApiUser> _signInManager;
    private readonly UserManager<ApiUser> _userManager;

    public AccountController(
        ApplicationDbContext context,
        ILogger<DomainsController> logger,
        IConfiguration configuration,
        UserManager<ApiUser> userManager,
        SignInManager<ApiUser> signInManager)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    ///     Registers a new user
    /// </summary>
    /// <param name="input">A DTO containing the user data</param>
    /// <returns>A 201 - Created Status Code in case of success</returns>
    /// <response code="201">User has been registered</response>
    /// <response code="400">Invalid data</response>
    /// <response code="500">An error occurred</response>
    [ProducesResponseType(typeof(string), 201)]
    [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    [SwaggerOperation(Tags = new[] { "Auth" })] // grouping endpoints
    [HttpPost]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult> Register(RegisterDTO input)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var newUser = new ApiUser();
                newUser.UserName = input.Username;
                newUser.Email = input.Email;

                var result = await _userManager.CreateAsync(newUser, input.Password!);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {userName} ({email}) has been created.", newUser.UserName,
                        newUser.Email);

                    return StatusCode(201, $"User '{newUser.UserName}' has been created.");
                }

                throw new Exception(string.Format("Error: {0}",
                    string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            var details = new ValidationProblemDetails(ModelState);
            details.Type =
                "https:/ /tools.ietf.org/html/rfc7231#section-6.5.1";
            details.Status = StatusCodes.Status400BadRequest;

            return new BadRequestObjectResult(details);
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails();
            exceptionDetails.Detail = e.Message;
            exceptionDetails.Status =
                StatusCodes.Status500InternalServerError;
            exceptionDetails.Type =
                "https:/ /tools.ietf.org/html/rfc7231#section-6.6.1";
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                exceptionDetails);
        }
    }

    /// <summary>
    ///     Performs a user login
    /// </summary>
    /// <param name="input">A DTO containing the user's credentials</param>
    /// <returns>The Bearer Token</returns>
    /// <response code="200">User has been logged in</response>
    /// <response code="400">Login failed (Bad Request)</response>
    /// <response code="401">Login failed (unauthorized)</response>
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [SwaggerOperation(Tags = new[] { "Auth" })]
    [HttpPost]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult> Login(LoginDTO input)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(input.UserName!);

                if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password!))
                    throw new Exception("Invalid login attempt.");

                var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]!)), SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.Name, user.UserName!));

                var jwtObject = new JwtSecurityToken(
                    _configuration["JWT:Issuer"],
                    _configuration["JWT:Audience"],
                    claims,
                    expires: DateTime.Now.AddSeconds(300),
                    signingCredentials: signingCredentials);

                var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);

                return StatusCode(StatusCodes.Status200OK, jwtString);
            }

            var details = new ValidationProblemDetails(ModelState);

            details.Type = "https:/ /tools.ietf.org/html/rfc7231#section-6.5.1";

            details.Status = StatusCodes.Status400BadRequest;

            return new BadRequestObjectResult(details);
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails();

            exceptionDetails.Detail = e.Message;
            exceptionDetails.Status = StatusCodes.Status401Unauthorized;
            exceptionDetails.Type = "https:/ /tools.ietf.org/html/rfc7231#section-6.6.1";

            return StatusCode(StatusCodes.Status401Unauthorized, exceptionDetails);
        }
    }
}