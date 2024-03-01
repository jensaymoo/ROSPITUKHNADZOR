using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandHandlerAttribute : Attribute
    {
        public string? CommandName { get; private set; }
        public ChatType ChatType { get; private set; }

        public CommandHandlerAttribute(string? commandName, ChatType chatType)
        {
            CommandName = commandName;
            ChatType = chatType;
        }
    }
}