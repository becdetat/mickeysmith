using System;
using System.Threading.Tasks;
using MickeySmith;

namespace MickeySmithTestbed.Commands.Set
{
    public class Test1Command : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\ttest1\t\tTest 1";
        public Type ParentCommandType => typeof(SetTestsCommand);
        public bool CanHandle(string command) => command.IsRoughly("test1");

        public Test1Command(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public Task Execute(string command)
        {
            using (new Benchmark())
            {
                var session = _sessionFactory();

                 session.Set("customer_1", new
                {
                    name = "Tokiflonk Twistgrinder",
                    currency = "AUD"
                });
            }

            return Task.CompletedTask;
        }
    }
}