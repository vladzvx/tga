using Telegram.Bot;
using TGA.CoreLib.Bot.Messages.Interfaces;

namespace TGA.CoreLib.Bot.Messages
{
    public class SendedItemFactory : ISendedItemFactory
    {
        private readonly ITelegramBotClient _telegramBotClient;
        public SendedItemFactory(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public ISendedItem Create(string text, long targetChat)
        {
            return new TextMessage(targetChat, text, _telegramBotClient);
        }
    }
}
