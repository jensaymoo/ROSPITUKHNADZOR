namespace rospitukhnadzor
{
    internal static class Extensions
    {
        public static string ToBytesString(this int integer) => BitConverter.ToString(BitConverter.GetBytes(integer));
    }
}
