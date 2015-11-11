using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace JsonDatabase
{
    // Largely lifted from DannyBoy - https://github.com/swxben/danny-boy
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

        public Task ExecuteCommandAsync(string commandText, object parameters = null) => ExecuteCommandAsync(commandText, parameters, ConnectionString);
        public Task ExecuteCommandOnMasterAsync(string commandText, object parameters = null) => ExecuteCommandAsync(commandText, parameters, MasterConnectionString);

        private static async Task ExecuteCommandAsync(string commandText, object parameters, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = CreateCommand(connection, commandText, parameters))
                {
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

        static SqlCommand CreateCommand(SqlConnection connection, string sql, object parameters)
        {
            var command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = sql;


            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.GetType().GetProperties().Select(x => GetCommandParameter(command, x.Name, x.GetValue(parameters, null))).ToArray());
                command.Parameters.AddRange(parameters.GetType().GetFields().Select(x => GetCommandParameter(command, x.Name, x.GetValue(parameters))).ToArray());
            }

            return command;
        }

        static SqlParameter GetCommandParameter(SqlCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = $"@{name}";

            if (value != null && value.GetType().IsEnum)
            {
                value = value.ToString();
            }

            parameter.Value = value ?? DBNull.Value;

            var s = value as string;
            if (s != null)
            {
                parameter.Size = s.Length > 4000 ? -1 : 4000;
            }

            return parameter;
        }
    }
}