using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MickeySmith;

namespace MickeySmithTestbed.Commands.IncrTests
{
    public class IncrMultiThreadCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;
        private const string Key = "incrmultithread";

        public IncrMultiThreadCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public string Description => "\tincrmultithread\tTest incrementing on 5 threads";
        public Type ParentCommandType => typeof (IncrTestsCommand);
        public bool CanHandle(string command) => command.IsRoughly("incrmultithread");

        public Task Execute(string command)
        {
            using (new Benchmark())
            {
                var session = _sessionFactory();

                session.Set(Key, 123);
                Cmd.WriteInfoLine("initial value is 123");

                var tasks = Enumerable.Range(0, 5)
                    .Select(_ => Task.Run(() =>
                    {
                        session.Incr(Key);
                        Cmd.WriteLine($"incr {_}");
                    }))
                    .ToArray();
                Task.WaitAll(tasks);

                var finalValue = session.Get(Key);

                Cmd.WriteInfoLine($"final value is {finalValue}");

                session.Delete(Key);

                return Task.CompletedTask;
            }
        }
    }
}
