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
            var dbCount = _danny.ExecuteScalarOnMaster($"SELECT COUNT(*) FROM [sys].[databases] WHERE [Name] = '{_danny.DatabaseName}'");
            return dbCount > 0;
        }

        public void DropDatabase()
        {
            _danny.ExecuteCommandOnMaster($@"ALTER DATABASE [{_danny.DatabaseName}] SET single_user WITH rollback immediate");
            _danny.ExecuteCommandOnMaster($"DROP DATABASE [{_danny.DatabaseName}]");
        }

        public void CreateDatabase()
        {
            _danny.ExecuteCommandOnMaster($"CREATE DATABASE [{_danny.DatabaseName}]");
            this.PerformMigrations();
        }

        private void PerformMigrations()
        {
            _danny.ExecuteCommand(@"
CREATE TABLE JsonStore(
    [Key] NVARCHAR(MAX) NOT NULL,
    [Value] NVARCHAR(MAX)
)
                ");
        }
    }
}