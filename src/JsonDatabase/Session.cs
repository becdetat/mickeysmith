using System;
using System.Linq;
using Newtonsoft.Json;

namespace MickeySmith
{
    public class Session
    {
        private const string _jsonStoreTableName = "[dbo].[JsonStore]";
        private readonly Danny _danny;

        public Session(string connectionString)
        {
            _danny = new Danny(connectionString);
        }

        private Session(Danny danny)
        {
            _danny = danny;
        }

        public void Set(string key, object value)
        {
            // TODO check for invalid wildcards in the key

            var valueJson = JsonConvert.SerializeObject(value);
            var command = $@"
IF (EXISTS(SELECT [Key] FROM {_jsonStoreTableName} WHERE [Key] = @key)) BEGIN
    UPDATE [JsonStore] SET [Value] = @value WHERE [Key] = @key
END ELSE BEGIN 
    INSERT INTO [JsonStore]([Key], [Value]) VALUES(@key, @value)
END
            ";

            _danny.ExecuteCommand(command, new
            {
                key,
                value = valueJson
            });
        }

        public dynamic Get(string key)
        {
            var results = _danny.ExecuteQuery($"SELECT [Value] FROM {_jsonStoreTableName} WHERE [Key] = @key", new {key}).ToArray();

            if (!results.Any()) return null;

            var json = results.First().Value;

            dynamic value = JsonConvert.DeserializeObject(json);

            return value;
        }

        public IQuery Query(string query)
        {
            return new Query(_danny, query);
        }

        public long Incr(string key, long amount = 1)
        {
            long result = 0;
            _danny.ExecuteTransaction(System.Data.IsolationLevel.RepeatableRead, danny =>
            {
                var session = new Session(danny);
                var current = session.Get(key);

                if (!(current is long))
                {
                    throw new InvalidOperationException("Only integer values can be incremented or decremented");
                }

                result = current + amount;
                session.Set(key, result);

                danny.Commit();
            });

            return result;
        }

        public long Decr(string key, long amount = 1)
        {
            return Incr(key, -amount);
        }

        public void Delete(string key)
        {
            var command = $"DELETE FROM {_jsonStoreTableName} WHERE [Key] = @key";

            _danny.ExecuteCommand(command, new {key});
        }
    }
}