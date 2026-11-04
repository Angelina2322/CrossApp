using Core;
using Core.Domain;
using Core.Reports;
using System.Diagnostics;

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

    case "bench":
        RunBenchmark();
        break;

    default:
        Console.WriteLine("report-top | report-low | report-groups | report-sum | demo-deferred | bench");
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

static void RunBenchmark()
{
    Console.WriteLine("Генерація 100 000 записів для тестування...");
    var random = new Random(42);
    var largeDataset = Enumerable.Range(1, 100_000)
        .Select(i => Product.Create(
            $"id-{i}",
            $"SKU-{i:D6}",
            $"Товар {i}",
            i % 2 == 0 ? "шт" : "м",
            random.Next(1, 1000)))
        .ToList();

    Console.WriteLine("Запуск тестів (100 000 записів)...");

    // Варіант А: Чистий потоковий ланцюжок (без зайвого ToList посередині)
    var sw = Stopwatch.StartNew();
    var resultA = largeDataset
        .Where(p => p.Quantity < 500)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .OrderByDescending(p => p.Quantity)
        .Take(10)
        .ToList(); // фінальна матеріалізація
    sw.Stop();
    var elapsedStreaming = sw.ElapsedMilliseconds;

    // Варіант Б: Із проміжним ToList() у середині ланцюжка
    sw.Restart();
    var resultB = largeDataset
        .Where(p => p.Quantity < 500)
        .ToList() // ПРОМІЖНИЙ ToList(): виділяє масив під десятки тисяч елементів!
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .OrderByDescending(p => p.Quantity)
        .Take(10)
        .ToList();
    sw.Stop();
    var elapsedWithMiddleToList = sw.ElapsedMilliseconds;

    Console.WriteLine($"\n--- Результати замірів (Stopwatch) ---");
    Console.WriteLine($"1. Без ToList посередині (streaming):  {elapsedStreaming} ms");
    Console.WriteLine($"2. З проміжним ToList() у середині:     {elapsedWithMiddleToList} ms");
    Console.WriteLine($"Різниця: варіант без ToList() швидший і не створює зайвих алокацій у GC.");
}