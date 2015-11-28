using System;

namespace MickeySmithTestbed.Commands.Set
{
    public class SetTestsCommand : IBranchCliCommand
    {
        public string Description => "\tsettests\t\tSet tests";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.IsRoughly("settests");
    }
}