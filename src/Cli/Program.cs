using System.Text;
using Cli.Sinks;          
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

var store = new InMemoryCatalogStore();
var service = new CatalogService(store);

var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
var logDir = Path.GetDirectoryName(logPath);
if (!string.IsNullOrEmpty(logDir))
{
    Directory.CreateDirectory(logDir);
}

// Перший підписник (локальний метод)
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;

// Другий підписник (лямбда із замиканням)
int changes = 0;
service.Changed += (_, _) => changes++;

Console.WriteLine("--- Додавання та зміна залишку ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");

Console.WriteLine("\n--- Відписка (-=) ---");
service.Changed -= OnCatalogChanged;

service.Receive(created.Id, 5);
Console.WriteLine($"Подій отримано: {changes}");

Console.WriteLine("\n--- Сценарій помилки ---");
try
{
    service.Receive(created.Id, -10);
}
catch (Exception ex)
{
    Console.WriteLine($"[Очікуваний виняток]: {ex.Message}");
}
Console.WriteLine($"Подій після помилки: {changes}");

Console.WriteLine("\n--- Пошук за допомогою Func ---");
var found = service.FindAll(p => p.Quantity > 50);
foreach (var product in found)
{
    Console.WriteLine($"Знайдено: {product.Name}, залишок: {product.Quantity}");
}

var sinks = new INotificationSink[]
{
    new ConsoleSink(),
    new FileSink(logPath)
};

var serviceWithSinks = new CatalogServiceWithSinks(store, sinks);

var rawData = new[]
{
    "SKU-201;Патч-корд 1м;шт;10",
    "НЕКОРЕКТНИЙ_РЯДОК_БЕЗ_РОЗДІЛЬНИКІВ",
    "SKU-202;Кабель живлення;шт;-5",
    "SKU-203;Коннектор RJ-45;уп;100"
};

int imported = service.ImportFromLines(rawData, warning =>
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"[ПОПЕРЕДЖЕННЯ ІМПОРТУ]: {warning}");
    Console.ResetColor();
});

Console.WriteLine($"Успішно імпортовано записів: {imported}");