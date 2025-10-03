using MySql.Data.MySqlClient;

namespace gameshop_api.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = "Server=202.28.34.203;Port=3306;Database=mb68_66011212118;Uid=mb68_66011212118;Pwd=CbJmLDN7hjTM;";
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}