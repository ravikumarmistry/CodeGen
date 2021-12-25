using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using CodeGen.Commands;
using CodeGen.Exceptions;
using CodeGen.Handlers;
using CommandLine;
using CommandLine.Text;
using Serilog;

namespace CodeGen
{
    class Program
    {
        public static int Main(string[] args)
        {
            IContainer? Container = BuildContainer(args);
            var commandDispatcher = Container.Resolve<ICommandDispatcher>();
            try
            {
                commandDispatcher.DispatchAsync(args).Wait();
            }
            catch (HumanizedException ex)
            {
                if (ex.Action == HumanizedExceptionAction.InformUserAndExit)
                {
                    Console.WriteLine(ex.Message);
                    // log exception
                    Log.Error(ex, ex.Message);
                    return 1;
                }
                else if (ex.Action == HumanizedExceptionAction.InformUserAndThrow)
                {
                    // something unexpected has happened, inform user and throw and log the exception
                    Log.Error(ex, ex.Message);
                    throw;
                }
                else if (ex.Action == HumanizedExceptionAction.InformUserAndContinue)
                {
                    // User has done something wrong, inform user and continue
                    Console.WriteLine(ex.Message);
                    return 0;
                }
                else if (ex.Action == HumanizedExceptionAction.Hide)
                {
                    return 0;
                }
            }
            catch (HookCompileException)
            {
                // users template code has exception do nothing
                throw;
            }
            catch (Exception ex)
            {
                // ask user to create a bug report with log file
                Log.Error(ex, ex.Message);
                Console.WriteLine($"Please create a bug report at 'https://github.com/ravikumarmistry/CodeGen/issues' with the log file '{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")}'.");
                throw;
            }
            return 0;
        }

        private static void ConfigureLogging()
        {
            // log in the folder where the executable is
            var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 1)
                .CreateLogger();
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
