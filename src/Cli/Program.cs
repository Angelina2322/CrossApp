using System;
using System.IO;
using System.Threading;
using Core;
using Core.Services;
using Core.Storage;

// 1. Ініціалізуємо сховище та сервіс (ручний DI з попередніх робіт)
var store = new FileCatalogStore("data/catalog.json"); // або JsonCatalogStore / InMemoryCatalogStore, як у вас названо
var service = new CatalogService(store);

// 2. Налаштування токена скасування та перехоплення Ctrl+C
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Для перевірки таймауту за потреби:
// cts.CancelAfter(TimeSpan.FromSeconds(2));

string dataPath = args.Length > 0 ? args[0] : "data/import.json";

try
{
    var importer = new JsonCatalogImporter();
    var added = await service.ImportAsync(importer, dataPath, cts.Token);
    Console.WriteLine($"Імпортовано записів: {added}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Імпорт скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"Файл не знайдено: {dataPath}");
}