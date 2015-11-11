using System;
using System.Threading.Tasks;
using JsonDatabase;

namespace JsonDatabaseTestbed.Commands.Set
{
    public class Test1Command : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\ttest1\t\tTest 1";
        public Type ParentCommandType => typeof(SetCommand);
        public bool CanHandle(string command) => command.IsRoughly("test1");

        public Test1Command(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task Execute(string command)
        {
            using (new Benchmark())
            {
                var session = _sessionFactory();

                await session.SetAsync("customer_1", new
                {
                    name = "Tokiflonk Twistgrinder",
                    currency = "AUD"
                });
            }
        }
    }
}