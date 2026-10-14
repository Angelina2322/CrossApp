using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

// Composition Root: ручне впровадження залежностей (Manual DI)
ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"=== Сховище: {store.GetType().Name} ===");

try
{
    // Демонстрація операцій
    Console.WriteLine("\n-> Додаємо новий товар:");
    var created = service.Add("SKU-999", "Мережевий адаптер USB", "шт", 20);
    Console.WriteLine($"Створено: [{created.Id}] {created.Name} ({created.Quantity} {created.Unit})");

    Console.WriteLine("\n-> Оприбуткування (Receive +15):");
    service.Receive(created.Id, 15);

    Console.WriteLine("\n-> Список товарів у сховищі (перші 5):");
    foreach (var p in service.All().Take(5))
    {
        Console.WriteLine($"  {p.Id,-10} {p.Sku,-10} {p.Name,-28} {p.Quantity,5} {p.Unit}");
    }

    // Сценарій відмови (демонстрація обробки винятків)
    Console.WriteLine("\n-> Тест відмови: спроба оприбуткування неіснуючого товару:");
    service.Receive("unknown-id-000", 10);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[ПОМИЛКА БІЗНЕС-ЛОГІКИ]: {ex.Message}");
    Console.ResetColor();
}