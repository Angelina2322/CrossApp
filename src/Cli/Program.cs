using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

// Завдання 1: вибір імпортера за розширенням
if (extension == ".json")
{
    ImportResult<ProductDto> jsonResult = ProductJsonImporter.Load(path);
    DisplayResult(jsonResult, item => $"{item.Id,-6} {item.Sku,-10} {item.Name,-25} {item.Quantity,5} {item.Unit}");
    PrintStatistics(jsonResult);
    return 0;
}

// Завдання 2: якщо файл mixed.csv — запускаємо змішаний імпортер
if (Path.GetFileName(path).Equals("mixed.csv", StringComparison.OrdinalIgnoreCase))
{
    ImportResult<IEntityDto> mixedResult = MixedCsvImporter.Load(path);
    DisplayResult(mixedResult, item => item switch
    {
        ProductDto p => $"[Товар] {p.Id,-6} {p.Sku,-10} {p.Name,-22} {p.Quantity,4} {p.Unit}",
        WarehouseDto w => $"[Склад] {w.Id,-6} {w.Code,-10} {w.Location}",
        _ => item.ToString() ?? string.Empty
    });
    PrintStatistics(mixedResult);
    return 0;
}

// Стандартний CSV імпорт
ImportResult<ProductDto> csvResult = ProductCsvImporter.Load(path);
DisplayResult(csvResult, p => $"{p.Id,-6} {p.Sku,-10} {p.Name,-25} {p.Quantity,5} {p.Unit}");

// Завдання 3: статистика імпорту одним рядком
PrintStatistics(csvResult);

return 0;

static void DisplayResult<T>(ImportResult<T> result, Func<T, string> format)
{
    Console.WriteLine($"Завантажено записів: {result.Items.Count}");
    foreach (T item in result.Items.Take(5))
    {
        Console.WriteLine($"  {format(item)}");
    }

    if (result.Errors.Count > 0)
    {
        Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
        foreach (string error in result.Errors)
        {
            Console.WriteLine($"  ! {error}");
        }
    }
}

// Завдання 3: функція статистики
static void PrintStatistics<T>(ImportResult<T> result)
{
    int accepted = result.Items.Count;
    int skipped = result.Errors.Count;
    int total = accepted + skipped;
    double errorRate = total > 0 ? (double)skipped / total * 100 : 0.0;

    Console.WriteLine($"\nСтатистика: Усього: {total} | Прийнято: {accepted} | Пропущено: {skipped} | Помилок: {errorRate:F1}%\n");
}