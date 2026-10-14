using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogStore
{
    IReadOnlyList<Product> List();
    Product? GetById(string id);
    IReadOnlyList<Product> FindAll(Func<Product, bool> predicate); // Додаткове завдання 2
    void Add(Product item);
    void Update(Product item);
    bool Remove(string id);
}