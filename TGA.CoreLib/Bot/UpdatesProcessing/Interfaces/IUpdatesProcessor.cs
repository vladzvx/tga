using Telegram.Bot.Types;

namespace TGA.CoreLib.Bot.UpdatesProcessing.Interfaces
{
    public interface IUpdatesProcessor
    {
        public Task Process(Update update);
    }
}
