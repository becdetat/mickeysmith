using System;
using System.Threading.Tasks;
using JsonDatabase;

namespace JsonDatabaseTestbed.Commands.Set
{
    public class Test2Command : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        public string Description => "\ttest2\t\tTest 2";
        public Type ParentCommandType => typeof(SetCommand);
        public bool CanHandle(string command) => command.IsRoughly("test2");

        public Test2Command(Func<Session> sessionFactory)
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
                    name = "Carbnus Fastneedle",
                    currency = "NZD"
                });
            }
        }
    }
}