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

        public static async Task<T> InvokeAsync<T>(this MethodInfo methodInfo, object obj, params object[] parameters)
        {
            dynamic awaitable = methodInfo.Invoke(obj, parameters);
            await awaitable;
            return (T)awaitable.GetAwaiter().GetResult();
        }

        public static async Task InvokeAsync(this MethodInfo methodInfo, object obj, params object[] parameters)
        {
            dynamic awaitable = methodInfo.Invoke(obj, parameters);
            await awaitable;
        }
    }
}
