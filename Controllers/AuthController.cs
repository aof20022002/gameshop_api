using gameshop_api.Data;
using gameshop_api.Models.req_res;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace gameshop_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public AuthController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT uid, email, password, fullname, profile_image, role FROM User WHERE email = @email";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", request.Email);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return BadRequest(new { message = "Email หรือรหัสผ่านไม่ถูกต้อง" });
                }

                var passwordFromDb = reader.GetString("password");


                if (request.Password != passwordFromDb)
                {
                    return BadRequest(new { message = "Email หรือรหัสผ่านไม่ถูกต้อง" });
                }

                return Ok(new
                {
                    message = "Login สำเร็จ",
                    uid = reader.GetInt32("uid"),
                    email = reader.GetString("email"),
                    fullname = reader.GetString("fullname"),
                    profile_image = reader.IsDBNull(reader.GetOrdinal("profile_image")) ? null : reader.GetString("profile_image"),
                    role = reader.GetString("role")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var isConnected = await _db.TestConnection();
            return Ok(new
            {
                message = "Auth API is working!",
                dbConnected = isConnected,
                timestamp = DateTime.Now
            });
        }
    }
}