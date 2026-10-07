using System;
using Core.Dto; // Підключення простору імен DTO для мапінгу (Крок 6)

namespace Core.Domain;

public sealed class Product
{
    // Крок 2: Приватне поле для інкапсуляції стану залишку
    private int _quantity;

    // Крок 2: Властивості лише для читання (жодних public set чи init)
    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }
    public int Quantity => _quantity;

    // Крок 2: Приватний конструктор — пряме створення через new заборонено
    private Product(string id, string sku, string name, string unit, int quantity)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        _quantity = quantity;
    }

    // Крок 3 та 5: Фабричний метод, який перевіряє ВСІ аргументи ДО створення
    public static Product Create(string id, string sku, string name, string unit, int quantity)
    {
        // Інваріант 1: обов'язкові рядкові поля не порожні
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU не може бути порожнім", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва не може бути порожньою", nameof(name));

        // Інваріант 2: початковий залишок не може бути від'ємним
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Початковий залишок не може бути від'ємним");

        // Нормалізація рядків перед збереженням
        return new Product(
            id.Trim(),
            sku.Trim().ToUpperInvariant(),
            name.Trim(),
            unit.Trim(),
            quantity);
    }

    // Кроки 4 та 5: Бізнес-метод приходу товару
    public void RegisterArrival(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість приходу має бути більшою за нуль");

        _quantity += amount;
    }

    // Кроки 4 та 5: Бізнес-метод видачі товару з перевіркою інваріанту стану
    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount,
                "Кількість видачі має бути більшою за нуль");

        // Інваріант 3: операція неможлива в поточному стані (перевитрата)
        if (amount > _quantity)
            throw new InvalidOperationException(
                $"Не можна видати {amount}: залишок {Sku} = {_quantity}");

        _quantity -= amount;
    }

    // Крок 6: Мапінг сутності у формат тижня 3 (DTO)
    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity);

    // Крок 6: Відновлення сутності з DTO (обов'язково проходить через Create і ті самі перевірки)
    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);

    public override string ToString() => $"{Id} [{Sku}] {Name} - {Quantity} {Unit}";
}