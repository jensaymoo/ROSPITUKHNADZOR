namespace RosPitukhNadzor
{
    internal class BanWord
    {
        public BanWord() { }
        public BanWord(long chat, string word)
        {
            ChatID = chat;
            Word = word;
        }
        public long ChatID { get; set; }
        public string Word { get; set; }
    }
}
