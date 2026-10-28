using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogServiceWithSinks
{
    private readonly ICatalogStore _store;
    private readonly IReadOnlyList<INotificationSink> _sinks;

    public CatalogServiceWithSinks(ICatalogStore store, IEnumerable<INotificationSink> sinks)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sinks = (sinks ?? throw new ArgumentNullException(nameof(sinks))).ToList();
    }

    private void NotifySinks(Product p, CatalogChangeKind kind, int delta)
    {
        var args = new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity);
        foreach (var sink in _sinks)
        {
            sink.Notify(args);
        }
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        NotifySinks(product, CatalogChangeKind.Added, quantity);
        return product;
    }
}