using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonDatabase
{
    public class Session
    {
        private readonly Shorty _shorty;

        public Session(string connectionString)
        {
            _shorty = new Shorty(connectionString);
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

            return _shorty.ExecuteCommandAsync(command, new
            {
                key,
                value = valueJson
            });
        }

        public async Task<dynamic> Get(string key)
        {
            var results = (await _shorty.ExecuteQueryAsync("SELECT [Value] FROM [JsonStore] WHERE [Key] = @key",new {key})).ToArray();

            if (!results.Any()) return null;

            var json = results.First().Value;

            dynamic value = JsonConvert.DeserializeObject(json);

            return value;
        }

        public IQuery Query(string query)
        {
            return new Query(_shorty, query);
        }
    }
}