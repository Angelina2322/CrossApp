using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Product> Items() => new()
    {
        Product.Create("1", "SKU-BIG", "Кабель", "м", 50),
        Product.Create("2", "SKU-MID", "Шнур", "м", 20),
        Product.Create("3", "SKU-LOW", "Перехідник", "шт", 5),
        Product.Create("4", "SKU-ZERO", "Розетка", "шт", 0)
    };
}