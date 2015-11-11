using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonDatabase;

namespace JsonDatabaseTestbed.Commands
{
    public class QueryCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\tquery\t\tQuery";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.IsRoughly("query") || command.RoughlyStartsWith("query ");

        public QueryCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task Execute(string command)
        {
            var session = _sessionFactory();
            var query = GetQuery(command);

            IDictionary<string, dynamic> results;

            using (new Benchmark())
            {
                results = await session.Query(query).EndAsync();
            }

            if (!results.Any())
            {
                Cmd.WriteWarningLine("No results");
            }
            else
            {
                Cmd.WriteSubheader($"{results.Count} results");
                foreach (var result in results)
                {
                    Cmd.WriteLine($"Key: {result.Key}");
                    Cmd.WriteLine($"Type: {result.Value.GetType()}");
                    Cmd.WriteLine(result.Value.ToString());
                }
            }
        }

        static string GetQuery(string command)
        {
            var bits = command.Split(' ');
            if (bits.Length == 2)
            {
                return bits[1];
            }

            Cmd.WriteLine("Query:");

            return Cmd.Prompt();
        }
    }
}
