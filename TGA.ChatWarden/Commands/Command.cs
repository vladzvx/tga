using Telegram.Bot.Types;

namespace TGA.ChatWarden.Commands
{
    public class Command
    {
        public static readonly Command Empty = new Command();
        public Duration Duration { get; set; }

        public Execution Execution { get; set; }

        public long TargetChat { get; set; }

        public long TargetUser { get; set; }

        public string TargetUserName { get; set; } = string.Empty;

        public bool Report { get; set; }

        public bool DeleteMessages { get; set; }

        public bool IsEmpty => this == Empty;

        public static bool TryCreateCommand(Message message, out Command command)
        {
            command = Command.Empty;
            if (message != null && message.Chat != null && message.ReplyToMessage != null && message.ReplyToMessage.From != null && !string.IsNullOrEmpty(message.Text ?? message.Caption))
            {
                command = new()
                {
                    TargetChat = message.Chat.Id,
                    TargetUser = message.ReplyToMessage.From.Id,
                    TargetUserName = message.ReplyToMessage.From.FirstName
                };
                switch (message.Text ?? message.Caption)
                {
                    case "-a":
                        command.Duration = Duration.Forever;
                        command.Execution = Execution.AddAdmin;
                        return true;
                    case "-pr":
                        command.Duration = Duration.Forever;
                        command.Execution = Execution.Privillege;
                        return true;
                    case "-w":
                        command.Duration = Duration.Week;
                        command.Execution = Execution.Mute;
                        return true;
                    case "-d":
                        command.Duration = Duration.Day;
                        command.Execution = Execution.Mute;
                        return true;
                    case "-h":
                        command.Duration = Duration.Hour;
                        command.Execution = Execution.Mute;
                        return true;
                    case "-m":
                        command.Duration = Duration.Day;
                        command.Execution = Execution.NoMedia;
                        return true;
                    case "-b":
                        command.Duration = Duration.Forever;
                        command.Execution = Execution.Ban;
                        return true;
                    case "-w-p":
                        command.Duration = Duration.Week;
                        command.Execution = Execution.Mute;
                        command.Report = true;
                        return true;
                    case "-h-p":
                        command.Duration = Duration.Hour;
                        command.Execution = Execution.Mute;
                        command.Report = true;
                        return true;
                    case "-d-p":
                        command.Duration = Duration.Day;
                        command.Execution = Execution.Mute;
                        command.Report = true;
                        return true;
                    case "-m-p":
                        command.Duration = Duration.Day;
                        command.Execution = Execution.NoMedia;
                        command.Report = true;
                        return true;
                    case "-b-p":
                        command.Duration = Duration.Forever;
                        command.Execution = Execution.Ban;
                        command.Report = true;
                        return true;
                }
            }
            return false;
        }

        public string GetTextMessage(BotProfile botProfile)
        {
            string text = TargetUserName;
            if (Execution == Execution.NoMedia)
            {
                text += " ограничен(а) в отправке медиа";
            }
            else if (Execution == Execution.Mute)
            {
                text += " ограничен(а) в отправке сообщений";
            }
            else if (Execution == Execution.Ban)
            {
                return string.Format(botProfile.GetRandomBanReplica(), text);
            }

            if (Duration == Duration.Day)
            {
                text += " на сутки.";
            }
            else if (Duration == Duration.Hour)
            {
                text += " на час.";
            }
            else if (Duration == Duration.ThreeHours)
            {
                text += " на 3 часа.";
            }
            else if (Duration == Duration.Week)
            {
                text += " на неделю.";
            }
            else if (Duration == Duration.Forever)
            {
                text += " навсегда.";
            }
            return text;
        }
    }
}
