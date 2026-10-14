using Cli;
using Core.Services;

// Створення сховища через StoreFactory
var store = StoreFactory.Create(args);
var service = new CatalogService(store);

Console.WriteLine($"=== Сховище: {store.GetType().Name} ===");

try
{
    // Демонстрація предикатного пошуку (Func<Product, bool>)
    Console.WriteLine("\n-> Пошук товарів з залишком > 20 одиниць (Func<Product, bool>):");
    var surplus = service.Search(p => p.Quantity > 20);
    foreach (var p in surplus)
    {
        Console.WriteLine($"  [{p.Id}] {p.Name,-26} : {p.Quantity} {p.Unit}");
    }

    Console.WriteLine("\n-> Додаємо новий товар:");
    var created = service.Add("SKU-EXTRA", "Оптичний конектор", "шт", 100);
    Console.WriteLine($"Створено: [{created.Id}] {created.Name}");

    Console.WriteLine("\n-> Перевірка наявності в кеші/сховищі:");
    var found = service.Find(created.Id);
    Console.WriteLine($"Знайдено: {found?.Name} (кількість: {found?.Quantity})");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[ПОМИЛКА]: {ex.Message}");
    Console.ResetColor();
}