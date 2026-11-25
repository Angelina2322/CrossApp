using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore : ICatalogStore
{
    private readonly Dictionary<string, Product> _items;
    private readonly object _lock = new();

    public InMemoryCatalogStore(IEnumerable<Product>? seed = null)
    {
        _items = (seed ?? [])
            .ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Product> List()
    {
        lock (_lock)
            return _items.Values.ToList();
    }

    public Product? GetById(string id)
    {
        lock (_lock)
            return _items.GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_lock)
        {
            if (_items.ContainsKey(item.Id))
                throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");

            _items.Add(item.Id, item);
        }
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_lock)
        {
            if (!_items.ContainsKey(item.Id))
                throw new InvalidOperationException($"Запис з id={item.Id} не знайдено для оновлення.");

            _items[item.Id] = item;
        }
    }

    public bool Remove(string id)
    {
        lock (_lock)
            return _items.Remove(id);
    }

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        lock (_lock)
            return _items.Values.Where(predicate).ToList();
    }
}