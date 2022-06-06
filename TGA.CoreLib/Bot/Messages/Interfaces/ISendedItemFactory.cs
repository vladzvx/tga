namespace TGA.CoreLib.Bot.Messages.Interfaces
{
    public interface ISendedItemFactory
    {
        public ISendedItem Create(string text, long targetChat);
    }
}
