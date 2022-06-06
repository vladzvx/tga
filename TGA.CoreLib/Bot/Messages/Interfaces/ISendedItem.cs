using Telegram.Bot.Types;

namespace TGA.CoreLib.Bot.Messages.Interfaces
{
    public interface ISendedItem
    {
        public long ChatId { get; }
        public Task<Message> Send();
    }
}
