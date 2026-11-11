using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    private void OnChanged(Product p, CatalogChangeKind kind, int delta) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);

        OnChanged(product, CatalogChangeKind.Added, quantity);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();

    // Додаткове завдання 2: метод імпорту розміщено всередині класу
    public int ImportFromLines(IEnumerable<string> lines, Action<string>? onRowSkipped = null)
    {
        int importedCount = 0;
        int lineIndex = 0;

        foreach (var rawLine in lines)
        {
            lineIndex++;

            if (string.IsNullOrWhiteSpace(rawLine))
            {
                onRowSkipped?.Invoke($"Рядок {lineIndex}: порожній запис, пропущено.");
                continue;
            }

            var parts = rawLine.Split(';');
            if (parts.Length < 4)
            {
                onRowSkipped?.Invoke($"Рядок {lineIndex}: недостатньо колонок ('{rawLine}'), пропущено.");
                continue;
            }

            if (!int.TryParse(parts[3], out var quantity) || quantity < 0)
            {
                onRowSkipped?.Invoke($"Рядок {lineIndex}: некоректна або від'ємна кількість ('{parts[3]}'), пропущено.");
                continue;
            }

            Add(parts[0].Trim(), parts[1].Trim(), parts[2].Trim(), quantity);
            importedCount++;
        }

        return importedCount;
    }

public async Task<int> ImportAsync(ICatalogImporter importer, string path, CancellationToken ct = default)
{
    var items = await importer.LoadAsync(path, ct);
    var added = 0;

    foreach (var item in items)
    {
        ct.ThrowIfCancellationRequested();

        // Ідемпотентність імпорту: якщо товар уже є, пропускаємо його
        if (_store.GetById(item.Id) is not null)
            continue;

        _store.Add(item);
        OnChanged(item, CatalogChangeKind.Added, item.Quantity);
        added++;
    }

    return added;
}
public async Task<int> ImportStreamAsync(
    ICatalogImporter importer, 
    string path, 
    IProgress<int>? progress = null, 
    CancellationToken ct = default)
{
    var added = 0;

    await foreach (var item in importer.StreamAsync(path, ct))
    {
        ct.ThrowIfCancellationRequested();

        if (_store.GetById(item.Id) is not null)
            continue;

        _store.Add(item);
        OnChanged(item, CatalogChangeKind.Added, item.Quantity);
        added++;

        // Повідомляємо про прогрес після кожного доданого товару
        progress?.Report(added);
    }

    return added;
}
public async Task<int> ImportWithProgressAsync(
    ICatalogImporter importer, 
    string path, 
    IProgress<double>? progress = null, 
    CancellationToken ct = default)
{
    var items = await importer.LoadAsync(path, ct);
    var added = 0;
    var total = items.Count;

    for (var i = 0; i < total; i++)
    {
        ct.ThrowIfCancellationRequested();

        var item = items[i];
        if (_store.GetById(item.Id) is null)
        {
            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            added++;
        }

        // Рахуємо відсоток прогресу від 0.0 до 100.0%
        if (total > 0 && progress != null)
        {
            var percentage = (double)(i + 1) / total * 100.0;
            progress.Report(percentage);
        }
    }

    return added;
}
}