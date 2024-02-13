using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosPitukhNadzor
{
    internal class Warning
    {
        public Warning(long chat, long fromUser, long toUser, DateTime time)
        {
            ChatID = chat;
            FromUserID = fromUser;
            ToUserID = toUser;
            MuteExpiries = time;
        }
        public long ChatID;
        public long FromUserID;
        public long ToUserID;
        public DateTime MuteExpiries;
    }
}
