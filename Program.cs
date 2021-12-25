using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using CodeGen.Commands;
using CodeGen.Handlers;
using CommandLine;
using CommandLine.Text;


namespace CodeGen
{
    class Program
    {
        static IContainer? Container { get; set; }
        public static int Main(string[] args)
        {
            Container = BuildContainer(args);
            var commandDispatcher = Container.Resolve<ICommandDispatcher>();
            CommandDispatcher.Container = Container;
            commandDispatcher.DispatchAsync(args).Wait();
            return 0;
        }

        private static IContainer? BuildContainer(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<CommandDispatcher>().As<ICommandDispatcher>();
            builder.RegisterType<NewCommandHandler>().Named<ICommandHandler>(nameof(NewCommand));
            builder.RegisterType<GenCommandHandler>().Named<ICommandHandler>(nameof(GenCommand));

            return builder.Build();
        }
    }
}
