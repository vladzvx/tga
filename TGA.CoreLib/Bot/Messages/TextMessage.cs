using Telegram.Bot;
using Telegram.Bot.Types;
using TGA.CoreLib.Bot.Messages.Interfaces;

namespace TGA.CoreLib.Bot.Messages
{
    public class TextMessage : ISendedItem
    {
        public long ChatId { get; private set; }
        private readonly string _text;
        private readonly ITelegramBotClient _telegramBotClient;
        internal TextMessage(long chatId, string text, ITelegramBotClient telegramBotClient)
        {
            _text = text;
            _telegramBotClient = telegramBotClient;
            ChatId = chatId;
        }
        public async Task<Message> Send()
        {
            return await _telegramBotClient.SendTextMessageAsync(ChatId, _text);
        }
    }
}
