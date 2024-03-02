using Autofac;
using RosPitukhNadzor.Commands;
using System.Reflection;

namespace RosPitukhNadzor
{
    internal class Program
    {
        public static async Task Main()
        {
            var builder = new ContainerBuilder();
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


            using (var scope = builder.Build().BeginLifetimeScope())
            {
                await scope.Resolve<ITelegramBotInstance>().Run();
            }
        }
    }
}
