using System;
using System.Threading.Tasks;
using JsonDatabase;

namespace JsonDatabaseTestbed.Commands
{
    public class GetCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;

        public GetCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public string Description => "\tget\t\tGet";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.RoughlyStartsWith("get ");

        public async Task Execute(string command)
        {
            var session = _sessionFactory();

            var key = GetKey(command);

            dynamic result;

            using (new Benchmark())
            {
                result = await session.Get(key);
            }

            if (result == null)
            {
                Cmd.WriteWarningLine("No result");
            }
            else
            {
                Cmd.WriteLine($"Type: {result.GetType()}");
                Cmd.WriteLine(result.ToString());
            }
        }

        static string GetKey(string command)
        {
            var bits = command.Split(' ');
            if (bits.Length == 2)
            {
                return bits[1];
            }

            Cmd.WriteLine("Key:");

            return Cmd.Prompt();
        }
    }
}