using Autofac;
using RosPitukhNadzor.Commands;
using Serilog;
using System.Reflection;
using AutofacSerilogIntegration;

namespace RosPitukhNadzor
{
    internal class Program
    {
        public static async Task Main()
        {
            var builder = new ContainerBuilder();
            ILifetimeScope scope;
            ITelegramBotInstance instance;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss} {Level}] {Message} ({SourceContext:l}) {NewLine}{Exception}{NewLine}")               
                .CreateLogger();

            builder.RegisterLogger();

            try
            {
                builder.RegisterType<ConfigurationProviderJson>()
                    .As<IConfigurationProvider>()
                    .InstancePerLifetimeScope();

                builder.RegisterType<DatabaseStorageProvider>()
                    .As<IStorageProvider>()
                    .InstancePerLifetimeScope();

                builder.RegisterType<TelegramBot>()
                    .As<ITelegramBotInstance>()
                    .InstancePerLifetimeScope();

                builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                    .Where(t => t.GetCustomAttribute<MessageHandlerAttribute>() != null)
                    .As<IMessageHandler>()
                    .InstancePerLifetimeScope();

                scope = builder.Build().BeginLifetimeScope();
                instance = scope.Resolve<ITelegramBotInstance>();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal error during initialization.");
                return;
            }

            await instance.Run();
        }
    }
}
