using System.Data.SqlClient;
using System.Threading.Tasks;

namespace JsonDatabase
{
    public class Bootstrapper
    {
        private readonly Shorty _shorty;

        public Bootstrapper(string connectionString)
        {
            _shorty = new Shorty(connectionString);
        }

        public string ConnectionString => _shorty.ConnectionString;

        public bool DatabaseExists()
        {
            var task = _shorty.ExecuteScalarOnMasterAsync($"SELECT COUNT(*) FROM [sys].[databases] WHERE [Name] = '{_shorty.DatabaseName}'");

            task.Wait();

            return task.Result > 0;
        }

        public async Task DropDatabaseAsync()
        {
            await _shorty.ExecuteCommandOnMasterAsync($@"ALTER DATABASE [{_shorty.DatabaseName}] SET single_user WITH rollback immediate");
            await _shorty.ExecuteCommandOnMasterAsync($"DROP DATABASE [{_shorty.DatabaseName}]");
        }

        public async Task CreateDatabaseAsync()
        {
            await _shorty.ExecuteCommandOnMasterAsync($"CREATE DATABASE [{_shorty.DatabaseName}]");
            await this.PerformMigrationsAsync();
        }

        private Task PerformMigrationsAsync()
        {
            return _shorty.ExecuteCommandAsync(@"
CREATE TABLE JsonStore(
    [Key] NVARCHAR(MAX) NOT NULL,
    [Value] NVARCHAR(MAX)
)
                ");
        }
    }

    public class Session
    {
        public Session(string connectionString)
        {
        }
    }

    internal class Shorty
    {
        public Shorty(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        private string MasterConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString) {InitialCatalog = "master"};
                return builder.ConnectionString;
            }
        }

        public string DatabaseName
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);
                return builder.InitialCatalog;
            }
        }

        public Task ExecuteCommandAsync(string commandText) => ExecuteCommandAsync(commandText, ConnectionString);
        public Task ExecuteCommandOnMasterAsync(string commandText) => ExecuteCommandAsync(commandText, MasterConnectionString);

        private static async Task ExecuteCommandAsync(string commandText, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public Task<dynamic> ExecuteScalarAsync(string commandText) => ExecuteScalarAsync(commandText, ConnectionString);
        public Task<dynamic> ExecuteScalarOnMasterAsync(string commandText) => ExecuteScalarAsync(commandText, MasterConnectionString);

        private static async Task<dynamic> ExecuteScalarAsync(string commandText, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;

                    dynamic result = await command.ExecuteScalarAsync();

                    return result;
                }
            }
        }
    }
}