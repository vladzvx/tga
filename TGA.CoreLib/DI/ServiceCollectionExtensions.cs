using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Telegram.Bot;
using TGA.CoreLib.Bot;
using TGA.CoreLib.Bot.Messages;
using TGA.CoreLib.Bot.Messages.Interfaces;
using TGA.CoreLib.Bot.UpdatesProcessing;
using TGA.CoreLib.Bot.UpdatesProcessing.Interfaces;
using TGA.CoreLib.Repositories;
using TGA.CoreLib.Repositories.Interfaces;

namespace TGA.CoreLib.DI
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Requires env variables TOKEN and MONGO_DB_CNNSTR
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static IServiceCollection AddBotComponents(this IServiceCollection services)
        {
            string token = Environment.GetEnvironmentVariable("TOKEN") ??
                throw new ApplicationException("Bot token dose not exist in env variable!");
            string mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_DB_CNNSTR") ??
                throw new ApplicationException("MONGO_DB_CNNSTR dose not exist in env variable!");

            TelegramBotClient client = new(token);
            MongoClient mongo = new(mongoConnectionString);
            if (client.BotId.HasValue)
            {
                IMongoDatabase db = mongo.GetDatabase(client.BotId.Value.ToString());
                services.AddSingleton<IMongoDatabase>(db);
            }
            else
            {
                throw new ApplicationException("Bot client creation failed!");
            }
            services.AddSingleton<ITelegramBotClient>(client);
            services.AddSingleton(mongo);
            services.AddSingleton<ISender, MessagesSender>();
            services.AddSingleton<ISendedItemFactory, SendedItemFactory>();
            services.AddSingleton<ICustomUpdateHandler, CustomUpdateHandler>();
            services.AddSingleton<IGenericRepository, GenericRepository>();
            services.AddSingleton<IUpdatesProcessor, UpdatesProcessorDefault>();
            services.AddHostedService<BotStarter>();
            return services;
        }
    }
}
