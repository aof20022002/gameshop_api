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
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.Fullname))
                {
                    return BadRequest(new { message = "กรุณากรอกข้อมูลให้ครบถ้วน" });
                }


                if (!request.Email.Contains("@"))
                {
                    return BadRequest(new { message = "รูปแบบอีเมลไม่ถูกต้อง" });
                }
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                var checkQuery = "SELECT COUNT(*) FROM User WHERE email = @email";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@email", request.Email);

                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (count > 0)
                {
                    return BadRequest(new { message = "อีเมลนี้ถูกใช้งานแล้ว" });
                }

                // Insert new user
                var profileImage = string.IsNullOrWhiteSpace(request.ProfileImage) ? "default.png" : request.ProfileImage;

                var insertQuery = @"INSERT INTO User (email, password, fullname, profile_image, role) 
                                   VALUES (@email, @password, @fullname, @profileImage, 'user')";

                using var insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@email", request.Email);
                insertCmd.Parameters.AddWithValue("@password", request.Password); // ไม่ hash ตามที่ต้องการ
                insertCmd.Parameters.AddWithValue("@fullname", request.Fullname);
                insertCmd.Parameters.AddWithValue("@profileImage", profileImage);

                await insertCmd.ExecuteNonQueryAsync();

                var getIdQuery = "SELECT LAST_INSERT_ID()";
                using var getIdCmd = new MySqlCommand(getIdQuery, connection);
                var newUid = Convert.ToInt32(await getIdCmd.ExecuteScalarAsync());

                return Ok(new
                {
                    message = "สมัครสมาชิกสำเร็จ",
                    uid = newUid,
                    email = request.Email,
                    fullname = request.Fullname,
                    profile_image = profileImage,
                    role = "user"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
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