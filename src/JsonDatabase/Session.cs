using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MickeySmith
{
    public class Session
    {
        private readonly Danny _danny;

        public Session(string connectionString)
        {
            _danny = new Danny(connectionString);
        }

        public Task SetAsync(string key, object value)
        {
            // TODO check for invalid wildcards in the key

            var valueJson = JsonConvert.SerializeObject(value);
            var command = $@"
IF (EXISTS(SELECT [Key] FROM [JsonStore] WHERE [Key] = @key)) BEGIN
    UPDATE [JsonStore] SET [Value] = @value WHERE [Key] = @key
END ELSE BEGIN 
    INSERT INTO [JsonStore]([Key], [Value]) VALUES(@key, @value)
END
            ";

            return _danny.ExecuteCommandAsync(command, new
            {
                key,
                value = valueJson
            });
        }

        public async Task<dynamic> Get(string key)
        {
            var results = (await _danny.ExecuteQueryAsync("SELECT [Value] FROM [JsonStore] WHERE [Key] = @key",new {key})).ToArray();

            if (!results.Any()) return null;

            var json = results.First().Value;

            dynamic value = JsonConvert.DeserializeObject(json);

            return value;
        }

        public IQuery Query(string query)
        {
            return new Query(_danny, query);
        }
    }
}