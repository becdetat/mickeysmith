using System;
using System.Threading.Tasks;
using MickeySmith;

namespace MickeySmithTestbed.Commands
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

        public Task Execute(string command)
        {
            _bootstrapper.DropDatabase();
            _bootstrapper.CreateDatabase();

            return Task.CompletedTask;
        }
    }
}