using Core.Domain;

namespace Core.Services;

public static class ReportService
{
    // Повертає товари, залишок яких менший за вказаний поріг
    public static IReadOnlyList<Product> LowStock(IEnumerable<Product> items, int threshold)
    {
        return items
            .Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .ToList();
    }

    // Детермінований топ-N за спаданням кількості
    public static IReadOnlyList<Product> TopByQuantity(IEnumerable<Product> items, int count)
    {
        return items
            .OrderByDescending(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Take(count)
            .ToList();
    }
}