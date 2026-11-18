using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileCatalogStore : ICatalogStore
{
    private readonly string _path;
    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public FileCatalogStore(string fullFilePath)
    {
        if (string.IsNullOrWhiteSpace(fullFilePath))
            throw new ArgumentException("Шлях до файлу сховища не може бути порожнім.", nameof(fullFilePath));

        _path = Path.GetFullPath(fullFilePath);
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Товар із Id '{item.Id}' вже існує.");

        _cache[item.Id] = item;
        Flush();
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (!_cache.ContainsKey(item.Id))
            throw new KeyNotFoundException($"Товар із Id '{item.Id}' не знайдено.");

        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }

    /// <summary>
    /// Експортує всю колекцію у вказаний JSON-файл (атомарно).
    /// </summary>
    public int ExportToJson(string targetFilePath)
    {
        EnsureLoaded();
        WriteAtomic(Path.GetFullPath(targetFilePath), _cache.Values);
        return _cache.Count;
    }

    /// <summary>
    /// Імпортує всю колекцію із вказаного JSON-файлу та зберігає у основне сховище.
    /// </summary>
    public int ImportFromJson(string sourceFilePath)
    {
        var fullSource = Path.GetFullPath(sourceFilePath);
        if (!File.Exists(fullSource))
            throw new FileNotFoundException($"Файл для імпорту не знайдено: {fullSource}", fullSource);

        var imported = ReadProductsFromFile(fullSource);
        _cache.Clear();
        foreach (var product in imported)
        {
            _cache[product.Id] = product;
        }
        _loaded = true;
        Flush();

        return _cache.Count;
    }

    private void Flush()
    {
        WriteAtomic(_path, _cache.Values);
    }

    private static void WriteAtomic(string destinationPath, IEnumerable<Product> products)
    {
        var dtos = products.Select(product => product.ToDto()).ToList();
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tmp = destinationPath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmp, destinationPath, overwrite: true);
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        var products = ReadProductsFromFile(_path);
        _cache.Clear();
        foreach (var product in products)
        {
            _cache[product.Id] = product;
        }

        _loaded = true;
    }

    private static List<Product> ReadProductsFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var dtos = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];

            var result = new List<Product>(dtos.Count);
            foreach (var dto in dtos)
            {
                result.Add(Product.FromDto(dto));
            }
            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                $"Помилка формату JSON у файлі '{filePath}': {ex.Message}",
                ex);
        }
    }
}