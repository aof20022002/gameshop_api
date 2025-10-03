using gameshop_api.Models;
using MySql.Data.MySqlClient;
using BCrypt.Net;
using System.Data;

namespace gameshop_api.Services
{
    public class AuthService
    {
        private readonly DatabaseService _dbService;

        public AuthService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<ApiResponse> Register(RegisterRequest request)
        {
            try
            {
                using var connection = _dbService.GetConnection();
                await connection.OpenAsync();

                // ตรวจสอบว่า username หรือ email มีอยู่แล้วหรือไม่
                var checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @email";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@username", request.Username);
                checkCmd.Parameters.AddWithValue("@email", request.Email);

                var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
                if (exists)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Username or Email already exists"
                    };
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Insert user
                var insertQuery = @"INSERT INTO users (username, email, password_hash, created_at) 
                                   VALUES (@username, @email, @passwordHash, @createdAt);
                                   SELECT LAST_INSERT_ID();";

                using var insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@username", request.Username);
                insertCmd.Parameters.AddWithValue("@email", request.Email);
                insertCmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                insertCmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);

                var userId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

                var user = new User
                {
                    Id = userId,
                    Username = request.Username,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow
                };

                return new ApiResponse
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = user
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> Login(LoginRequest request)
        {
            try
            {
                using var connection = _dbService.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT id, username, email, password_hash, created_at FROM users WHERE username = @username";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@username", request.Username);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                var user = new User
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Password = reader.GetString("password_hash"),
                    CreatedAt = reader.GetDateTime("created_at")
                };

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                // ไม่ส่ง password กลับไป
                user.Password = string.Empty;

                return new ApiResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Data = user
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> GetAllUsers()
        {
            try
            {
                using var connection = _dbService.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT id, username, email, created_at FROM users";
                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var users = new List<User>();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32("id"),
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                        CreatedAt = reader.GetDateTime("created_at")
                    });
                }

                return new ApiResponse
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}