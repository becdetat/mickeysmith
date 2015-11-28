using System;

namespace MickeySmithTestbed.Commands.IncrTests
{
    public class IncrTestsCommand : IBranchCliCommand
    {
        public string Description => "\tincrtests\tIncr tests";
        public Type ParentCommandType => null;
        public bool CanHandle(string command) => command.IsRoughly("incrtests");
    }
}