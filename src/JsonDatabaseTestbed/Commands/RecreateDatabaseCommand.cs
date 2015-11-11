using System;
using System.Threading.Tasks;
using JsonDatabase;

namespace JsonDatabaseTestbed.Commands
{
    public class RecreateDatabaseCommand : ILeafCliCommand
    {
        private readonly Bootstrapper _bootstrapper;

        public RecreateDatabaseCommand(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public string Description => "\trecreate\tRecreate database";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.IsRoughly("recreate");

        public async Task Execute(string command)
        {
            await _bootstrapper.DropDatabaseAsync();
            await _bootstrapper.CreateDatabaseAsync();
        }
    }
}