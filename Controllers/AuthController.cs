using Microsoft.AspNetCore.Mvc;
using gameshop_api.Models;
using gameshop_api.Services;

namespace gameshop_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly DatabaseService _dbService;

        public AuthController(AuthService authService, DatabaseService dbService)
        {
            _authService = authService;
            _dbService = dbService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var isConnected = await _dbService.TestConnection();
            return Ok(new
            {
                message = "Auth API is working!",
                dbConnected = isConnected,
                timestamp = DateTime.Now
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "All fields are required"
                });
            }

            var response = await _authService.Register(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            var response = await _authService.Login(request);

            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse>> GetAllUsers()
        {
            var response = await _authService.GetAllUsers();
            return Ok(response);
        }
    }
}