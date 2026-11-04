using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1) Фільтр + стабільне сортування + проєкція в DTO
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 2) Топ-N зі стабільним детермінованим порядком (додатковий ключ Sku)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Take(n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) Групування з підрахунком та сумою, сортування за спаданням суми
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity)
              .ToList();

    // 4) Агрегати з безпечною обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, 
               list.Sum(p => p.Quantity), 
               list.Average(p => p.Quantity), 
               list.Max(p => p.Quantity));
    }


// 5) GroupJoin: товари x переміщення з агрегацією
public static IReadOnlyList<ProductMovementSummary> MovementsByProduct(
    IEnumerable<Product> products,
    IEnumerable<StockMovement> movements) =>
    products.GroupJoin(
        movements,
        p => p.Sku,
        m => m.Sku,
        (product, productMovements) => new ProductMovementSummary(
            product.Sku,
            product.Name,
            product.Quantity,
            productMovements.Count(),
            productMovements.Sum(m => m.Quantity)
        ),
        StringComparer.OrdinalIgnoreCase
    )
    .OrderByDescending(p => p.TotalMovedQuantity)
    .ThenBy(p => p.Sku, StringComparer.Ordinal)
    .ToList();

    

    // Використання 1: Топ залишків
public static IReadOnlyList<StockLine> TopStockWithExtension(IEnumerable<Product> source, int n) =>
    source.TopBy(p => p.Quantity, p => p.Sku, n)
          .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
          .ToList();

// Використання 2: Топ переміщених товарів (для об'єднаного звіту)
public static IReadOnlyList<ProductMovementSummary> TopMovedProducts(
    IEnumerable<Product> products,
    IEnumerable<StockMovement> movements,
    int n) =>
    MovementsByProduct(products, movements)
        .TopBy(m => m.TotalMovedQuantity, m => m.Sku, n)
        .ToList();
        

        // Версія звітного методу ByUnit у синтаксисі запитів (Query Syntax)
public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
{
    var query = from p in source
                group p by p.Unit into g
                let total = g.Sum(x => x.Quantity)
                orderby total descending
                select new GroupSummary(g.Key, g.Count(), total);

    return query.ToList();
}

}