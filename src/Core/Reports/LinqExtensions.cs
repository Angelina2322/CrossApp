namespace Core.Reports;

public static class LinqExtensions
{
    // Узагальнений метод-розширення TopBy зі стабільним додатковим ключем
    public static IEnumerable<TSource> TopBy<TSource, TKey, TSecondaryKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TSecondaryKey> secondaryKeySelector,
        int count) =>
        source.OrderByDescending(keySelector)
              .ThenBy(secondaryKeySelector)
              .Take(count);
}