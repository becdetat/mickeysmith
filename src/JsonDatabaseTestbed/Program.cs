using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MickeySmith;
using MickeySmithTestbed.Commands;

namespace MickeySmithTestbed
{
    public class Program
    {
        public const string ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=MickeySmithTest;Integrated Security=True";

        private static void Main(string[] args)
        {
            IContainer container;

            try
            {
                Cmd.WriteHeader("Initialising...");

                var bootstrapper = new Bootstrapper(ConnectionString);

                container = IoC.HaveYouAnyIoC(bootstrapper);

                if (!bootstrapper.DatabaseExists()) bootstrapper.CreateDatabase();
            }
            catch (Exception ex)
            {
                Cmd.WriteException(ex);
                Cmd.Pause();
                throw;
            }

            var commands = container
                .Resolve<IEnumerable<ICliCommand>>()
                .ToArray();
            var quitCommand = commands.OfType<QuitCommand>().Single();

            CliAppLoop.StartAppLoop(commands, quitCommand);
        }
    }
}