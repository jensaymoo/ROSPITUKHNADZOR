using RosPitukhNadzor.Commands;
using System.Linq.Expressions;
using System.Reflection;

namespace RosPitukhNadzor
{
    internal static class Extensions
    {
        public static string ToBytesString(this int integer) => BitConverter.ToString(BitConverter.GetBytes(integer));

        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (T item in @this)
            {
                action(item);
            }
        }

        public static IEnumerable<IMessageHandler> GetHandlers(this IEnumerable<IMessageHandler> handlers, Expression<Func<MessageHandlerAttribute, bool>> expression) =>
            handlers.Where(x => x.GetType().GetCustomAttributes<MessageHandlerAttribute>().Where(expression.Compile()).Any());

    }
}
