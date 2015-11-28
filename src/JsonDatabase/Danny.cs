using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading;

namespace MickeySmith
{
    // Largely lifted from DannyBoy - https://github.com/swxben/danny-boy
    // This will be DannyPink - https://github.com/bendetat/dannypink
    internal class Danny : IDisposable
    {
        private readonly Func<ConnectionWrapper> _connectionFactory;
        private readonly Func<ConnectionWrapper> _masterConnectionFactory;
        private SqlTransaction _transaction;
        private bool _transactionComplete = false;

        public Danny(string connectionString)
            : this(null, null, null)
        {
            ConnectionString = connectionString;
        }

        private Danny(SqlTransaction transaction, Func<ConnectionWrapper> connectionFactory, Func<ConnectionWrapper> masterConnectionFactory)
        {
            _transaction = transaction;
            _connectionFactory = connectionFactory ?? (() =>
            {
                var connection = new SqlConnection(ConnectionString);

                const int maxAttempts = 10;
                int attempt = 0;
                var exceptions = new List<Exception>();
                var random = new Random();
                while (true)
                {
                    attempt++;
                    try
                    {
                        connection.Open();
                        break;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        if (attempt == maxAttempts)
                        {
                            throw new AggregateException($"Maximum number of attempts reached ({maxAttempts}) when opening connection", exceptions.ToArray());
                        }
                        Thread.Sleep(random.Next(200, 500));
                    }
                }

                return new ConnectionWrapper(connection, true);
            });
            _masterConnectionFactory = masterConnectionFactory ?? (() =>
            {
                var connection = new SqlConnection(MasterConnectionString);

                connection.Open();

                return new ConnectionWrapper(connection, true);
            });
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

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
                _connectionFactory().AllowDispose();
                _connectionFactory().Dispose();
            }
        }

        public void ExecuteCommand(string sql, object parameters = null) => ExecuteCommand(sql, parameters, _connectionFactory);
        public void ExecuteCommandOnMaster(string sql, object parameters = null) => ExecuteCommand(sql, parameters, _masterConnectionFactory);

        private void ExecuteCommand(string sql, object parameters, Func<ConnectionWrapper> connectionFactory)
        {
            using (var connection = connectionFactory())
            using (var command = this.CreateCommand(connection.Connection, sql, parameters))
            {
                command.ExecuteNonQuery();
            }
        }

        public dynamic ExecuteScalar(string sql, object parameters = null) => ExecuteScalar(sql, parameters, _connectionFactory);
        public dynamic ExecuteScalarOnMaster(string sql, object parameters = null) => ExecuteScalar(sql, parameters, _masterConnectionFactory);

        private dynamic ExecuteScalar(string sql, object parameters, Func<ConnectionWrapper> connectionFactory)
        {
            using (var connection = connectionFactory())
            {
                using (var command = this.CreateCommand(connection.Connection, sql, parameters))
                {
                    dynamic result = command.ExecuteScalar();

                    return result;
                }
            }
        }

        public IEnumerable<dynamic> ExecuteQuery(string sql, object parameters = null) => ExecuteQuery(sql, parameters, _connectionFactory);
        public IEnumerable<dynamic> ExecuteQueryOnMaster(string sql, object parameters = null) => ExecuteQuery(sql, parameters, _masterConnectionFactory);

        private IEnumerable<dynamic> ExecuteQuery(string sql, object parameters, Func<ConnectionWrapper> connectionFactory)
        {
            var results = new List<dynamic>();

            using (var connection = connectionFactory())
            {
                using (var command = this.CreateCommand(connection.Connection, sql, parameters))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = (new ExpandoObject()) as IDictionary<string, object>;

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

        public void ExecuteTransaction(
            System.Data.IsolationLevel isolationLevel,
            Action<Danny> transactionPayload,
            int maximumAttempts = 30)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot create non-nested transactions per connection");
            }

            var attempt = 0;
            var exceptions = new List<Exception>();
            var random = new Random();

            while (true)
            {
                attempt++;
                try
                {
                    var connection = _connectionFactory();

                    connection.DisallowDispose();

                    var transaction = connection.Connection.BeginTransaction(isolationLevel);
                    var danny = new Danny(transaction, () => connection, null);

                    transactionPayload(danny);

                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);

                    if (attempt == maximumAttempts)
                    {
                        throw new AggregateException(
                            $"Maximum number of attempts reached ({maximumAttempts}) when executing transaction payload",
                            exceptions);
                    }

                    Thread.Sleep(random.Next(200, 500));
                }
            }
        }

        public void Commit()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Can't commit without an active transaction");
            }

            if (_transactionComplete)
            {
                throw new InvalidOperationException("Transaction is already complete");
            }

            _transactionComplete = true;
            _transaction.Commit();
            _connectionFactory().AllowDispose();
        }

        public void Rollback()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Can't roll back without an active transaction");
            }

            if (_transactionComplete)
            {
                throw new InvalidOperationException("Transaction is already complete");
            }

            _transactionComplete = true;
            _transaction.Rollback();
            _connectionFactory().AllowDispose();
        }

        private SqlCommand CreateCommand(SqlConnection connection, string sql, object parameters)
        {
            var command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Transaction = this._transaction;


            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.GetType().GetProperties().Select(x => GetCommandParameter(command, x.Name, x.GetValue(parameters, null))).ToArray());
                command.Parameters.AddRange(parameters.GetType().GetFields().Select(x => GetCommandParameter(command, x.Name, x.GetValue(parameters))).ToArray());
            }

            return command;
        }

        private static SqlParameter GetCommandParameter(SqlCommand command, string name, object value)
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

        private class ConnectionWrapper : IDisposable
        {
            private bool _allowDispose;

            public ConnectionWrapper(SqlConnection connection, bool allowDispose)
            {
                _allowDispose = allowDispose;
                Connection = connection;
            }

            public SqlConnection Connection { get; }

            public void Dispose()
            {
                if (_allowDispose)
                {
                    Connection.Close();
                    Connection.Dispose();
                }
            }

            public void AllowDispose()
            {
                _allowDispose = true;
            }

            public void DisallowDispose()
            {
                _allowDispose = false;
            }
        }
    }
}