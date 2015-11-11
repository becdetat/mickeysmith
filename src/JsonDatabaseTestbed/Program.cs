using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JsonDatabase;
using JsonDatabaseTestbed.Commands;

namespace JsonDatabaseTestbed
{
    internal class Program
    {
        private const string ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=JsonDatabaseTest;Integrated Security=True";

        private static void Main(string[] args)
        {
            IContainer container;

            try
            {
                Cmd.WriteHeader("Initialising...");

                var bootstrapper = new Bootstrapper(ConnectionString);

                container = IoC.HaveYouAnyIoC(bootstrapper);

                if (!bootstrapper.DatabaseExists()) bootstrapper.CreateDatabaseAsync().Wait();
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