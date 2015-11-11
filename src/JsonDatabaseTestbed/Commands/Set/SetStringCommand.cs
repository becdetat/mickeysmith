using System;
using System.Threading.Tasks;
using MickeySmith;

namespace MickeySmithTestbed.Commands.Set
{
    public class SetStringCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\tstring\t\tSet string";
        public Type ParentCommandType => typeof(SetCommand);
        public bool CanHandle(string command) => command.IsRoughly("string");

        public SetStringCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task Execute(string command)
        {
            Cmd.WriteLine("Key:");
            var key = Cmd.Prompt();

            Cmd.WriteLine("Value:");
            var value = Cmd.Prompt();

            using (new Benchmark())
            {
                var session = _sessionFactory();

                await session.SetAsync(key, value);
            }
        }
    }
}