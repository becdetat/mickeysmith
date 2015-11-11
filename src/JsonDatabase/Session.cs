using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonDatabase
{
    public class Session
    {
        private readonly Shorty _shorty;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public Session(string connectionString)
        {
            _shorty = new Shorty(connectionString);
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public Task SetAsync(string key, object value)
        {
            // TODO check for invalid wildcards in the key

            var valueJson = JsonConvert.SerializeObject(value, _jsonSerializerSettings);
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
    }
}