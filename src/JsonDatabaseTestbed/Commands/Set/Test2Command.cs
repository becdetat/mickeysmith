using System;
using System.Threading.Tasks;
using MickeySmith;

namespace MickeySmithTestbed.Commands.Set
{
    public class Test2Command : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\ttest2\t\tTest 2";
        public Type ParentCommandType => typeof(SetTestsCommand);
        public bool CanHandle(string command) => command.IsRoughly("test2");

        public Test2Command(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public  Task Execute(string command)
        {
            using (new Benchmark())
            {
                var session = _sessionFactory();

                session.Set("customer_1", new
                {
                    name = "Carbnus Fastneedle",
                    currency = "NZD"
                });
            }

            return Task.CompletedTask;
        }
    }
}