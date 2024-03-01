namespace RosPitukhNadzor
{
    internal class Warning
    {
        public Warning() { }
        public Warning(long chat, long fromUser, long toUser, DateTime time)
        {
            ChatID = chat;
            FromUserID = fromUser;
            ToUserID = toUser;
            WarningExpiried = time;
        }
        public long ChatID;
        public long FromUserID;
        public long ToUserID;
        public DateTime WarningExpiried;
    }
}
