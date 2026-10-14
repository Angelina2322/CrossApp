using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _innerStore;
    private readonly Dictionary<string, Product> _memoryCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isInitialized;

    public CachingCatalogStore(ICatalogStore innerStore)
    {
        _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    }

    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        foreach (var item in _innerStore.List())
        {
            _memoryCache[item.Id] = item;
        }

        _isInitialized = true;
    }

    public IReadOnlyList<Product> List()
    {
        EnsureInitialized();
        return _memoryCache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureInitialized();
        return _memoryCache.GetValueOrDefault(id);
    }

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureInitialized();
        return _memoryCache.Values.Where(predicate).ToList();
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureInitialized();

        _innerStore.Add(item);
        _memoryCache[item.Id] = item;
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureInitialized();

        _innerStore.Update(item);
        _memoryCache[item.Id] = item;
    }

    public bool Remove(string id)
    {
        EnsureInitialized();

        var removed = _innerStore.Remove(id);
        if (removed)
        {
            _memoryCache.Remove(id);
        }

        return removed;
    }
}