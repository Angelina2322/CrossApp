using System;
using System.IO;
using System.Threading;
using Core;
using Core.Services;
using Core.Storage;

var store = new InMemoryCatalogStore(); // або FileCatalogStore
var service = new CatalogService(store);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

string dataPath = args.Length > 0 ? args[0] : "data/products.json";

// Підписник на прогрес: оновлює один і той самий рядок у консолі
var progress = new Progress<double>(percent =>
{
    Console.Write($"\rПрогрес імпорту: {percent:F1}%");
});

try
{
    var importer = new JsonCatalogImporter();
    var added = await service.ImportWithProgressAsync(importer, dataPath, progress, cts.Token);
    
    Console.WriteLine($"\nІмпорт успішно завершено! Додано записів: {added}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nІмпорт скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine($"\nДані відхилено: {ex.Message}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"\nФайл не знайдено: {dataPath}");
}