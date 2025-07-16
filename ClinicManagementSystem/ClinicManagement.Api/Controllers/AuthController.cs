using ClinicManagement.Api.DTOs.Auth;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClinicManagement.Api.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger; // Added for logging
        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: api/<AuthController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // In AuthController.cs, in your Register method
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.Register(registerDto); // This returns IdentityResult

            if (!result.Succeeded)
            {
                // Return errors from IdentityResult
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            // Successfully registered. You don't get the User object back directly here
            // from the AuthService's current implementation, but you can say it's successful.
            return Ok(new { message = "User registered successfully." });

            // If you needed the User ID, you'd have to modify AuthService.Register to return User or UserId
            // Example if AuthService returns User:
            /*
            var newUser = await _authService.Register(registerDto);
            if (newUser == null) return BadRequest(...); // Handle duplicate username/email
            return Ok(new { UserId = newUser.Id, Username = newUser.UserName, Email = newUser.Email });
            */
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="loginDto">User login credentials.</param>
        /// <returns>A JWT token on successful login or an unauthorized error.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Username and Password are required.");
            }

            try
            {
                var token = await _authService.Login(loginDto);

                if (token == null)
                {
                    return Unauthorized("Invalid username, password, or inactive account.");
                }

                _logger.LogInformation("User logged in: {Username}", loginDto.Username);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login for {Username}", loginDto.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during login.");
            }
        }
    }
}
