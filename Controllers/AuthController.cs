using gameshop_api.Data;
using gameshop_api.Models.req_res;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BCrypt.Net;
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

                var query = "SELECT uid, email, password, fullname, phone, role FROM User WHERE email = @email";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", request.Email);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return BadRequest(new { message = "Email หรือรหัสผ่านไม่ถูกต้อง" });
                }

                var passwordHash = reader.GetString("password");


                if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
                {
                    return BadRequest(new { message = "Email หรือรหัสผ่านไม่ถูกต้อง" });
                }

                return Ok(new
                {
                    message = "Login สำเร็จ",
                    uid = reader.GetInt32("uid"),
                    email = reader.GetString("email"),
                    fullname = reader.GetString("fullname"),
                    phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                    role = reader.GetString("role")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpPost("admin/login")]
        public async Task<IActionResult> AdminLogin(LoginRequest request)
        {
            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT uid, email, password, fullname, phone, role FROM User WHERE email = @email AND role = 'admin'";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", request.Email);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return BadRequest(new { message = "คุณไม่มีสิทธิ์เข้าถึงระบบ Admin" });
                }

                var passwordHash = reader.GetString("password");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
                {
                    return BadRequest(new { message = "Email หรือรหัสผ่านไม่ถูกต้อง" });
                }

                return Ok(new
                {
                    message = "Admin Login สำเร็จ",
                    uid = reader.GetInt32("uid"),
                    email = reader.GetString("email"),
                    fullname = reader.GetString("fullname"),
                    phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                    role = reader.GetString("role")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                // ตรวจสอบ email ซ้ำ
                var checkQuery = "SELECT COUNT(*) FROM User WHERE email = @email";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@email", request.Email);
                var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
                if (exists)
                {
                    return BadRequest(new { message = "Email นี้มีผู้ใช้แล้ว" });
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Insert user 
                var insertQuery = @"INSERT INTO User (email, password, fullname, phone, role)
                            VALUES (@email, @password, @fullname, @phone, 'user');
                            SELECT LAST_INSERT_ID();";
                using var insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@email", request.Email);
                insertCmd.Parameters.AddWithValue("@password", passwordHash);
                insertCmd.Parameters.AddWithValue("@fullname", request.Fullname);
                insertCmd.Parameters.AddWithValue("@phone", request.Phone ?? (object)DBNull.Value);

                var uid = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

                return Ok(new { message = "สมัครสมาชิกสำเร็จ", uid = uid, email = request.Email, fullname = request.Fullname, role = "user" });
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