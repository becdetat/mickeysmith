using System.Reflection;
using Autofac;
using MickeySmith;

namespace MickeySmithTestbed
{
    public static class IoC
    {
        public static IContainer HaveYouAnyIoC(Bootstrapper bootstrapper)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(x => x.IsAssignableTo<ICliCommand>())
                .As<ICliCommand>()
                .InstancePerDependency();
            builder
                .RegisterInstance(bootstrapper)
                .AsSelf();
            builder
                .Register(_ => new Session(bootstrapper.ConnectionString))
                .AsSelf();

            return builder.Build();
        }
    }
}