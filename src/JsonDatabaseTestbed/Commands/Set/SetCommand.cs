using System;
using System.Threading.Tasks;
using JsonDatabase;

namespace JsonDatabaseTestbed.Commands.Set
{
    public class SetCommand : IBranchCliCommand
    {
        public string Description => "\tset\t\tSet tests";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.IsRoughly("set");
    }

    public class Test1Command : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\ttest1\t\tTest 1";
        public Type ParentCommandType => typeof (SetCommand);
        public bool CanHandle(string command) => command.IsRoughly("test1");

        public Test1Command(Func<Session> sessionFactory )
        {
            _sessionFactory = sessionFactory;
        }

        public Task Execute()
        {
            var session = _sessionFactory();

            return Task.CompletedTask;
        }
    }

}