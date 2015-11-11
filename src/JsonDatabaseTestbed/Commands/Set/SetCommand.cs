using System;

namespace MickeySmithTestbed.Commands.Set
{
    public class SetCommand : IBranchCliCommand
    {
        public string Description => "\tset\t\tSet tests";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.IsRoughly("set");
    }
}