using System;
using System.Threading.Tasks;
using MickeySmith;

namespace MickeySmithTestbed.Commands.Set
{
    public class SetStringCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;

        public SetStringCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public string Description => "\tstring\t\tSet string";
        public Type ParentCommandType => typeof (SetTestsCommand);
        public bool CanHandle(string command) => command.IsRoughly("string");

        public Task Execute(string command)
        {
            Cmd.WriteLine("Key:");
            var key = Cmd.Prompt();

            Cmd.WriteLine("Value:");
            var value = Cmd.Prompt();

            using (new Benchmark())
            {
                var session = _sessionFactory();

                session.Set(key, value);
            }

            return Task.CompletedTask;
        }
    }
}