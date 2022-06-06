using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TGA.CoreLib.Bot.UpdatesProcessing.Interfaces;

namespace TGA.CoreLib.Bot
{
    public class BotStarter : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ICustomUpdateHandler _handler;
        private readonly ITelegramBotClient _telegramBotClient;
        public BotStarter(ITelegramBotClient telegramBotClient,
            ICustomUpdateHandler updateHandlerWithOptions)
        {
            _telegramBotClient = telegramBotClient;
            _handler = updateHandlerWithOptions;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _telegramBotClient.StartReceiving(_handler.HandleUpdateAsync,
                _handler.HandleErrorAsync,
                _handler.ReceiverOptions, _cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
