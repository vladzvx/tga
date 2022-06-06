using Telegram.Bot.Extensions.Polling;

namespace TGA.CoreLib.Bot.UpdatesProcessing.Interfaces
{
    public interface ICustomUpdateHandler : IUpdateHandler
    {
        public ReceiverOptions ReceiverOptions { get; }
    }
}
