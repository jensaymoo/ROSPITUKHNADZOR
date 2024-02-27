using Autofac;
using Newtonsoft.Json.Linq;
using RestSharp;
using rospitukhnadzor;
using System.Text.Json;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

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

            builder.RegisterType<WarningsStorageDatabase>()
                .As<IWarningsStorage>()
                .InstancePerLifetimeScope();

            builder.RegisterType<TelegramBot>()
                .As<ITelegramBotInstance>()
                .InstancePerLifetimeScope();


            using (var scope = builder.Build().BeginLifetimeScope())
            {
                await scope.Resolve<ITelegramBotInstance>().Run();
            }
        }
    }
}
