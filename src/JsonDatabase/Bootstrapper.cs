using System.Threading.Tasks;

namespace MickeySmith
{
    public class Bootstrapper
    {
        private readonly Danny _danny;

        public Bootstrapper(string connectionString)
        {
            _danny = new Danny(connectionString);
        }

        public string ConnectionString => _danny.ConnectionString;

        public bool DatabaseExists()
        {
            var task = _danny.ExecuteScalarOnMasterAsync($"SELECT COUNT(*) FROM [sys].[databases] WHERE [Name] = '{_danny.DatabaseName}'");

            task.Wait();

            return task.Result > 0;
        }

        public async Task DropDatabaseAsync()
        {
            await _danny.ExecuteCommandOnMasterAsync($@"ALTER DATABASE [{_danny.DatabaseName}] SET single_user WITH rollback immediate");
            await _danny.ExecuteCommandOnMasterAsync($"DROP DATABASE [{_danny.DatabaseName}]");
        }

        public async Task CreateDatabaseAsync()
        {
            await _danny.ExecuteCommandOnMasterAsync($"CREATE DATABASE [{_danny.DatabaseName}]");
            await this.PerformMigrationsAsync();
        }

        private Task PerformMigrationsAsync()
        {
            return _danny.ExecuteCommandAsync(@"
CREATE TABLE JsonStore(
    [Key] NVARCHAR(MAX) NOT NULL,
    [Value] NVARCHAR(MAX)
)
                ");
        }
    }
}