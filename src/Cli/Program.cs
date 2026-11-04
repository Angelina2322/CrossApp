using Core;
using Core.Domain;
using Core.Reports;

var products = SampleData.Products();

var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(products, 5));
        break;

    case "report-low":
        Print(ReportService.LowStock(products, 10));
        break;

    case "report-groups":
        foreach (var g in ReportService.ByUnit(products))
        {
            Console.WriteLine($"  {g.Key,-6} позицій: {g.Count,3}  сумарно: {g.TotalQuantity,6}");
        }
        break;

    case "report-sum":
        var t = ReportService.Totals(products);
        Console.WriteLine($"  позицій: {t.Items}, всього: {t.Total}, середнє: {t.Average:F2}, макс: {t.Max}");
        break;

    case "demo-deferred":
        RunDeferredExecutionDemo();
        break;

    default:
        Console.WriteLine("report-top | report-low | report-groups | report-sum | demo-deferred");
        break;
}

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-26} {l.Quantity,6} {l.Unit}");
    }
}

static void RunDeferredExecutionDemo()
{
    var source = SampleData.Products().ToList();
    var query = source.Where(p => p.Quantity < 10);

    Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

    source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3));
    Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

    var snapshot = query.ToList();
    source.Clear();

    Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
}