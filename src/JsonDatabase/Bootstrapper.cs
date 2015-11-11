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
}