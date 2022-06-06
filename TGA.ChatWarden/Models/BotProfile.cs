using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;

namespace TGA.ChatWarden
{
    public class BotProfile
    {
        [BsonId]
        public long Id { get; set; }
        public bool Overrun { get; set; }
        public List<ChatUserLink> Administrators { get; set; } = new List<ChatUserLink>();
        public List<ChatUserLink> PrivilegedUsers { get; set; } = new List<ChatUserLink>();
        public List<string> BannedWords { get; set; } = new List<string>();
        public List<string> StopWords { get; set; } = new List<string>();
        public string BannedSymbols { get; set; } = "";
        public string AllowedSymbols { get; set; } = "";
        public string HelpText { get; set; } = "Перечень команд: \n" +
                            "-b - бан.\n" +
                            "-w - ограничение отправки сообщений на неделю.\n" +
                            "-d - ограничение отправки сообщений на сутки.\n" +
                            "-h - ограничение отправки сообщений на час.\n" +
                            "-m - ограничение отправки медиа на сутки." +
                            "Команды с добавлением префикса -p - выводят отчет о результате действия.\n" +
                            "-pr - добавление пользователя в список привилегированных (не распространяются ограничения вводимые во время набегов).\n";
        public List<string> BanReplics { get; set; } = new List<string>();

        public string GetRandomBanReplica()
        {
            return BanReplics.Count > 0 ? BanReplics[RandomNumberGenerator.GetInt32(BanReplics.Count)] : "{0} забанен.";
        }
    }
}
