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
        public long ChatID { get; set; }
        public long FromUserID { get; set; }
        public long ToUserID { get; set; }
        public DateTime WarningExpiried { get; set; }
    }
}
