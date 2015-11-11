using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
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

        public Task ExecuteCommandAsync(string sql, object parameters = null) => ExecuteCommandAsync(sql, parameters, ConnectionString);
        public Task ExecuteCommandOnMasterAsync(string sql, object parameters = null) => ExecuteCommandAsync(sql, parameters, MasterConnectionString);

        private static async Task ExecuteCommandAsync(string sql, object parameters, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = CreateCommand(connection, sql, parameters))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public Task<dynamic> ExecuteScalarAsync(string sql, object parameters = null) => ExecuteScalarAsync(sql, parameters, ConnectionString);
        public Task<dynamic> ExecuteScalarOnMasterAsync(string sql, object parameters = null) => ExecuteScalarAsync(sql, parameters, MasterConnectionString);

        private static async Task<dynamic> ExecuteScalarAsync(string sql, object parameters, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = CreateCommand(connection, sql, parameters))
                {
                    dynamic result = await command.ExecuteScalarAsync();

                    return result;
                }
            }
        }

        public Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sql, object parameters = null) => ExecuteQueryAsync(sql, parameters, ConnectionString);
        public Task<IEnumerable<dynamic>> ExecuteQueryOnMasterAsync(string sql, object parameters = null) => ExecuteQueryAsync(sql, parameters, MasterConnectionString);

        static async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sql, object parameters, string connectionString)
        {
            var results = new List<dynamic>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = CreateCommand(connection, sql, parameters))
                    using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = (new ExpandoObject())  as IDictionary<string, object>;

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            item.Add(reader.GetName(i), DBNull.Value.Equals(reader[i]) ? null : reader[i]);
                        }

                        results.Add(item);
                    }
                }
            }

            return results;
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