using gameshop_api.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace gameshop_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public UserController(DatabaseHelper db)
        {
            _db = db;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                using var connection = _db.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT uid, fullname, email, phone, role FROM User";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var users = new List<object>();

                while (await reader.ReadAsync())
                {
                    users.Add(new
                    {
                        uid = reader.GetInt32("uid"),
                        fullname = reader.GetString("fullname"),
                        email = reader.GetString("email"),
                        phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                        role = reader.GetString("role")
                    });
                }

                return Ok(new { message = "เรียกข้อมูลผู้ใช้สำเร็จ", users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "เกิดข้อผิดพลาด", error = ex.Message });
            }
        }
    }
}