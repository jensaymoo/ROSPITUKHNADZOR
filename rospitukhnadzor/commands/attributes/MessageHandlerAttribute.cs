using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageHandlerAttribute : Attribute
    {
        public string? CommandName { get; private set; }
        public ChatType[] ChatTypes { get; private set; }

        public MessageHandlerAttribute(string? commandName, params ChatType[] chatType)
        {
            CommandName = commandName;
            ChatTypes = chatType;
        }
    }
}