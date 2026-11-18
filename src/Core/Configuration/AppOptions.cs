namespace Core.Configuration;

/// <summary>
/// Кореневий клас конфігурації застосунку.
/// </summary>
public sealed class AppOptions
{
    public string EnvironmentName { get; init; } = "Production";
    public LoggingOptions Logging { get; init; } = new();
    public StorageOptions Storage { get; init; } = new();
    public WarehouseOptions Warehouse { get; init; } = new();
}

/// <summary>
/// Налаштування рівня логування.
/// </summary>
public sealed class LoggingOptions
{
    public string MinimumLevel { get; init; } = "Information";
}

/// <summary>
/// Налаштування сховища даних.
/// </summary>
public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "items.json";

    /// <summary>
    /// Формує кросплатформний повний шлях до файлу даних та створює необхідний каталог.
    /// </summary>
    /// <param name="baseDir">Базовий каталог (наприклад, AppContext.BaseDirectory).</param>
    public string ResolveFullPath(string baseDir)
    {
        // Якщо DataPath вже є абсолютним шляхом — використовуємо його, інакше комбінуємо з baseDir
        var root = Path.IsPathRooted(DataPath)
            ? DataPath
            : Path.Combine(baseDir, DataPath);

        // Створюємо каталог у рантаймі, якщо він ще не існує
        Directory.CreateDirectory(root);

        return Path.GetFullPath(Path.Combine(root, FileName));
    }
}

/// <summary>
/// Доменні налаштування складу.
/// </summary>
public sealed class WarehouseOptions
{
    public string DefaultUnit { get; init; } = "шт";
}