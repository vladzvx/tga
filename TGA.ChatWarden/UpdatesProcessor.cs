using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TGA.ChatWarden.Commands;
using TGA.CoreLib.Bot.Messages.Interfaces;
using TGA.CoreLib.Bot.UpdatesProcessing.Interfaces;
using TGA.CoreLib.Repositories.Interfaces;

namespace TGA.ChatWarden
{
    public class UpdatesProcessor : IUpdatesProcessor
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ISender _sender;
        private readonly ISendedItemFactory _sendedItemFactory;
        private readonly IGenericRepository _genericRepository;

        public UpdatesProcessor(ITelegramBotClient telegramBotClient, ISender sender, ISendedItemFactory sendedItemFactory, IGenericRepository genericRepository)
        {
            _telegramBotClient = telegramBotClient;
            _sender = sender;
            _sendedItemFactory = sendedItemFactory;
            _genericRepository = genericRepository;
        }

        private async Task ProcessCommands(BotProfile profile, Message? message)
        {
            if (message != null && message.From != null && CheckAdmin(profile, message) && !string.IsNullOrEmpty(message.Text))
            {
                if (message.ReplyToMessage != null && message.ReplyToMessage.From != null && Command.TryCreateCommand(message, out Command command))
                {
                    DateTime? executionEnd = command.Duration == Duration.Forever ? null : DateTime.UtcNow.AddHours((int)command.Duration);
                    if (command.Execution == Execution.Mute)
                    {
                        await _telegramBotClient.RestrictChatMemberAsync(command.TargetChat, command.TargetUser, new ChatPermissions() { CanSendMessages = false }, executionEnd);
                    }
                    else if (command.Execution == Execution.Ban)
                    {
                        await _telegramBotClient.BanChatMemberAsync(command.TargetChat, command.TargetUser, executionEnd, true);
                        await _telegramBotClient.DeleteMessageAsync(message.Chat.Id, message.ReplyToMessage.MessageId);
                    }
                    else if (command.Execution == Execution.NoMedia)
                    {
                        await _telegramBotClient.RestrictChatMemberAsync(command.TargetChat, command.TargetUser, new ChatPermissions() { CanSendMediaMessages = true }, executionEnd);
                    }
                    else if (command.Execution == Execution.Privillege)
                    {
                        profile.PrivilegedUsers.Add(new ChatUserLink() {ChatId = command.TargetChat, UserId = command.TargetUser });
                        await _genericRepository.LogData(profile, f => f.Id == profile.Id);
                    }
                    else if (command.Execution == Execution.AddAdmin)
                    {
                        profile.Administrators.Add(new ChatUserLink() { ChatId = command.TargetChat, UserId = command.TargetUser });
                        await _genericRepository.LogData(profile, f => f.Id == profile.Id);
                    }
                    if (command.Report)
                    {
                        _sender.Add(_sendedItemFactory.Create(command.GetTextMessage(profile), command.TargetChat));
                    }
                }
                else
                {
                    if (message.Text.Contains("/protection_mode_on"))
                    {
                        profile.Overrun = true;
                        _sender.Add(_sendedItemFactory.Create("Активирован режим набега!", message.Chat.Id));
                    }
                    else if (message.Text.Contains("/protection_mode_off"))
                    {
                        profile.Overrun = false;
                        _sender.Add(_sendedItemFactory.Create("Деактивирован режим набега!", message.Chat.Id));
                    }
                    else if (message.Text.Contains("/help"))
                    {
                        _sender.Add(_sendedItemFactory.Create(profile.HelpText, message.Chat.Id));
                    }
                    else return;
                    await _genericRepository.LogData(profile, f => f.Id == profile.Id);
                }
                await _telegramBotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }
        }

        private async Task<BotProfile> GetProfile()
        {
            BotProfile profile;
            var profiles = await _genericRepository.GetData<BotProfile>(item => item.Id == _telegramBotClient.BotId);

            if (profiles != null && profiles.Count == 1)
            {
                profile = profiles[0];
            }
            else if (_telegramBotClient.BotId != null)
            {
                profile = new() { Id = _telegramBotClient.BotId.Value };
                await _genericRepository.LogData(profile);
            }
            else
            {
                throw new ApplicationException("Bot id is null!");
            }
            return profile;
        }

        private static bool CheckUpdate(Update update)
        {
            return update != null && update.Type == UpdateType.Message && update.Message != null && (update.Message.Chat.Type == ChatType.Supergroup || update.Message.Chat.Type == ChatType.Group) && !(string.IsNullOrEmpty(update.Message.Text ?? update.Message.Caption)) || (update!=null && update.Message != null && update.Message.NewChatMembers!=null && update.Message.NewChatMembers.Length>0);
        }

        public async Task Process(Update update)
        {
            if (CheckUpdate(update))
            {
                await _genericRepository.LogData(update.Message);
                var profile = await GetProfile();
                await ProcessCommands(profile, update.Message);
                if (update.Message != null && update.Message.From!=null && update.Message.From.Username == "Channel_Bot")
                {
                    await _telegramBotClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                }
                if (profile.Overrun && !CheckPrivilleged(profile, update.Message) && !CheckAdmin(profile, update.Message) && update.Message != null && update.Message.From!=null)
                {
                    if (CheckDeletion(profile,update.Message))
                    {
                        await _telegramBotClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                        return;
                    }

                    var cm = await _telegramBotClient.GetChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id);

                    if (cm.Status==ChatMemberStatus.Left || (update.Message.NewChatMembers != null && update.Message.NewChatMembers.Length>0))
                    {
                        await _telegramBotClient.BanChatMemberAsync(update.Message.Chat.Id, update.Message.From.Id, DateTime.UtcNow.AddHours(2), true);
                        await _telegramBotClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                        return;
                    }
                }
            }
        }

        private static string PrerairText(string text)
        {
            return text.Replace('c', 'с').Replace('p', 'р').Replace('y', 'у').Replace('e', 'е').Replace('x', 'х').Replace('a', 'а').Replace('k', 'к').Replace('o', 'о').Replace('ё', 'е').Replace("\n", "").Replace(" ", "");
        }

        private static bool CheckAdmin(BotProfile botProfile, Message? message)
        {
            return message != null && message.From != null && botProfile.Administrators.FindAll(item => item.ChatId == message.Chat.Id && item.UserId == message.From.Id).Count > 0;
        }

        private static bool CheckDeletion(BotProfile botProfile, Message message)
        {
            return false;
        }

        private static bool CheckPrivilleged(BotProfile botProfile, Message? message)
        {
            return message != null && message.From != null && botProfile.PrivilegedUsers.FindAll(item => item.ChatId == message.Chat.Id && item.UserId == message.From.Id).Count > 0;
        }
    }
}
