using System;
using System.Linq;
using System.Threading.Tasks;
using JsonDatabase;

namespace JsonDatabaseTestbed.Commands.Set
{
    public class SetArrayCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\tarray\t\tSet array";
        public Type ParentCommandType => typeof(SetCommand);
        public bool CanHandle(string command) => command.IsRoughly("array");

        public SetArrayCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task Execute(string command)
        {
            Cmd.WriteLine("Key:");
            var key = Cmd.Prompt();

            Cmd.WriteLine("Values (comma separated):");
            var value = Cmd.Prompt();
            var values = value.Split(',').Select(x => x.Trim()).ToArray();

            using (new Benchmark())
            {
                var session = _sessionFactory();

                await session.SetAsync(key, values);
            }
        }
    }
}