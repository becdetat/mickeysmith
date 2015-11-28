using System;
using System.Threading.Tasks;
using MickeySmith;

namespace MickeySmithTestbed.Commands
{
    public class IncrCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;

        public IncrCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public string Description => "\tincr\t\tIncr";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.RoughlyStartsWith("incr ");

        public Task Execute(string command)
        {
            var session = _sessionFactory();
            var key = GetKey(command);
            var value = GetValue(command);

            using (new Benchmark())
            {
                session.Incr(key, value);
            }

            return Task.CompletedTask;
        }

        private static string GetKey(string command)
        {
            var bits = command.Split(' ');
            if (bits.Length >= 2)
            {
                return bits[1];
            }

            Cmd.WriteLine("Key:");

            return Cmd.Prompt();
        }

        private static long GetValue(string command)
        {
            var bits = command.Split(' ');
            if (bits.Length >= 3)
            {
                var rest = command.Replace($"{bits[0]} {bits[1]} ", string.Empty);
                return long.Parse(rest);
            }

            Cmd.WriteLine(("Incr by:"));
            var json = Cmd.Prompt();

            return long.Parse(json);
        }
    }
}