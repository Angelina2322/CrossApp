using System.Reflection;
using System.Text;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = Encoding.UTF8;

try
{
    var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

    var config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false)
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
        .AddEnvironmentVariables(prefix: "CROSSAPP_")
        .AddCommandLine(args)
        .Build();

    var options = config.Get<AppOptions>() ?? new AppOptions();

    var legacyDataPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
    var effectiveStorage = new StorageOptions
    {
        DataPath = string.IsNullOrWhiteSpace(legacyDataPath) ? options.Storage.DataPath : legacyDataPath,
        FileName = options.Storage.FileName
    };

    var fullDataFilePath = effectiveStorage.ResolveFullPath(AppContext.BaseDirectory);
    var apiKey = config["Integration:ApiKey"];

    Console.WriteLine($"Файл даних: {fullDataFilePath}");
    Console.WriteLine(string.IsNullOrEmpty(apiKey)
        ? "ApiKey: не заданий"
        : $"ApiKey: присутній, довжина {apiKey.Length}");

    var store = new FileCatalogStore(fullDataFilePath);

    if (store.List().Count == 0)
    {
        store.Add(Product.Create("a1b2c3d4", "A-100", "Цемент М400", "мішок", 40));
        store.Add(Product.Create("e5f6g7h8", "B-205", "Цегла рядова", options.Warehouse.DefaultUnit, 500));
        store.Add(Product.Create("i9j0k1l2", "C-310", "Арматура 12мм", "м", 120));
    }

    var command = args.FirstOrDefault(a => !a.StartsWith('-'))?.ToLowerInvariant();

    switch (command)
    {
        case "export":
        {
            var targetPath = args.SkipWhile(a => a != "export").Skip(1).FirstOrDefault(a => !a.StartsWith('-'))
                             ?? fullDataFilePath;
            var count = store.ExportToJson(targetPath);
            Console.WriteLine($"Експортовано {count} записів.");
            break;
        }
        case "import":
        {
            var sourcePath = args.SkipWhile(a => a != "import").Skip(1).FirstOrDefault(a => !a.StartsWith('-'))
                             ?? fullDataFilePath;
            var count = store.ImportFromJson(sourcePath);
            Console.WriteLine($"Імпортовано {count} записів.");
            break;
        }
        default:
        {
            var items = store.List();
            Console.WriteLine($"У каталозі записів: {items.Count}");
            foreach (var item in items)
            {
                Console.WriteLine($" - [{item.Sku}] {item.Name}: {item.Quantity} {item.Unit}");
            }
            break;
        }
    }
}
catch (InvalidDataException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"Помилка читання даних: {ex.Message}");
    Console.ResetColor();
    Environment.ExitCode = 1;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"Помилка: {ex.Message}");
    Console.ResetColor();
    Environment.ExitCode = 1;
}