using System.Text;

namespace BlockScript.Utilities
{
    public static class EnumerableExtension
    {
        public static string Stringify<T>(this IEnumerable<T> source, string separator = ",")
        {
            var sb = new StringBuilder();
            foreach (var item in source)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(item);
            }

            return sb.ToString();
        }
        
        public static string Stringify<T>(this IEnumerable<T> source, Func<int, string> separator)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (var item in source)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator(index));
                }
                sb.Append(item);
                index++;
            }

            return sb.ToString();
        }

        public static T? FindOrNull<T>(this IEnumerable<T> source, Predicate<T> predicate) where T : struct
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return null;
        }
    }
}