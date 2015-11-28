using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MickeySmith;
using MickeySmithTestbed.Commands.Set;
using Newtonsoft.Json;

namespace MickeySmithTestbed.Commands
{
    public class SetCommand : ILeafCliCommand
    {
        private readonly Func<Session> _sessionFactory;

        public SetCommand(Func<Session> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public string Description => "\tset\t\tSet";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.RoughlyStartsWith("set ");

        public Task Execute(string command)
        {
            var session = _sessionFactory();
            var key = GetKey(command);
            var value = GetValue(command);

            using (new Benchmark())
            {
                session.Set(key, value);
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

        private static object GetValue(string command)
        {
            var bits = command.Split(' ');
            if (bits.Length >= 3)
            {
                var rest = command.Replace($"{bits[0]} {bits[1]} ", string.Empty);
                return JsonConvert.DeserializeObject(rest);
            }

            Cmd.WriteLine(("JSON:"));
            var json = Cmd.Prompt();

            return JsonConvert.DeserializeObject(json);
        }
    }
}
