using System.Reflection;

namespace RosPitukhNadzor
{
    internal static class Extensions
    {
        public static string ToBytesString(this int integer) => BitConverter.ToString(BitConverter.GetBytes(integer));

        public static void  ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (T item in @this)
            {
                action(item);
            }
        }
    }
}
