using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MickeySmith
{
    public interface IQuery
    {
        IDictionary<string, dynamic> End();
    }

    internal class Query : IQuery
    {
        private readonly Danny _danny;
        private readonly string _query;

        public Query(Danny danny, string query)
        {
            _danny = danny;
            _query = query;
        }

        public IDictionary<string, dynamic> End()
        {
            var sql = @"
SELECT [Key], [Value]
FROM [JsonStore]
WHERE [KEY] LIKE @pattern ESCAPE '~'
                ";
            var pattern = MakeQuerySafe();
            var results = _danny.ExecuteQuery(sql, new {pattern}).ToArray();

            return results
                .ToDictionary(
                    x => (string) x.Key,
                    x => JsonConvert.DeserializeObject(x.Value));
        }

        private string MakeQuerySafe()
        {
            // The query is used directly in a SQL LIKE statement, so we need to swap out the wildcards
            // and escape underscores (and tildes, which is used as the escape character).

            // Incoming wildcards are:
            //  * match 0 or more characters
            //  ? match 1 character

            return _query
                .Replace("~", "~~")
                .Replace("_", "~_")
                .Replace("?", "_")
                .Replace("*", "%");
        }
    }
}