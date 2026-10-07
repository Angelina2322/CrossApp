using System;
using Core.Domain;
using Core.Dto;

// Встановлюємо кодування для коректного виводу українських літер
Console.OutputEncoding = System.Text.Encoding.UTF8;

// ==========================================
// СЦЕНАРІЙ 1: УСПІХ
// ==========================================
Console.WriteLine("--- Сценарій 1: успіх ---");

// Створення коректного товару через фабричний метод
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);

// Успішні операції приходу та видачі
product.RegisterArrival(50);
product.Issue(30);

// Стан змінився: 100 + 50 - 30 = 120
Console.WriteLine(product);

Console.WriteLine();

// ==========================================
// СЦЕНАРІЙ 2: ПОРУШЕННЯ ІНВАРІАНТІВ
// ==========================================
Console.WriteLine("--- Сценарій 2: порушення інваріантів ---");

// 1. Порушення інваріанту стану: видача більша за залишок (1000 при залишку 120)
TryDo("видача більша за залишок", () => product.Issue(1000));

// 2. Порушення інваріанту вхідних даних: передача порожнього рядка замість SKU
TryDo("порожній SKU", () => Product.Create("P-002", "", "Пісок", "т", 10));

// 3. Порушення діапазону значень: початковий залишок менший за нуль
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));

Console.WriteLine();
Console.WriteLine($"Перевірка незмінності стану після помилок: {product}");

// ==========================================
// ДОПОМІЖНИЙ МЕТОД ДЛЯ ОБРОБКИ ВИКЛИКІВ
// ==========================================
static void TryDo(string title, Action action)
{
    try
    {
        action();
        // Якщо помилка не виникла — отже інваріант не спрацював
        Console.WriteLine($"[-] {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        // Виводимо лише назву винятку та зрозуміле повідомлення без стек-трейсу
        Console.WriteLine($"[+] {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("=== Додаткове завдання 1: Обробка ImportResult -> Domain ===");

// Симулюємо дані, отримані з CSV на тижні 3 (2 валідні, 2 з битими інваріантами)
var sampleDtos = new List<ProductDto>
{
    new("P-101", "SKU-101", "Цегла червона", "шт", 500),
    new("P-102", "", "Пісок річковий", "т", 15),                  // Порушення: порожній SKU
    new("P-103", "SKU-103", "Арматура 12мм", "м", -10),           // Порушення: від'ємна кількість
    new("P-104", "SKU-104", "Грунтовка глибокого проникнення", "л", 20)
};

// Припустимо, тиждень 3 повернув об'єкт ImportResult
var fakeImportResult = new ImportResult<ProductDto>(sampleDtos, []);
// Викликаємо доменну фільтрацію
DomainImportResult domainResult = ProductDomainImporter.ProcessImportResult(fakeImportResult);

Console.WriteLine($"Успішно імпортовано сутностей: {domainResult.Products.Count}");
foreach (var p in domainResult.Products)
{
    Console.WriteLine($"  [OK] {p}");
}

Console.WriteLine($"Відхилено рядків через порушення інваріантів: {domainResult.Errors.Count}");
foreach (var err in domainResult.Errors)
{
    Console.WriteLine($"  [ПОМИЛКА] Id: {err.RawDto.Id} ({err.ExceptionType}): {err.ErrorMessage}");
}

Console.WriteLine("=== Додаткове завдання 2: Інваріант на 2 сутності ===");

var reader = Reader.Create("R-01", "Олена Коваль");

// Видаємо 5 дозволених книг
for (int i = 1; i <= 5; i++)
{
    var copy = BookCopy.Create($"BC-0{i}", $"978-0-123456-0{i}");
    reader.BorrowBook(copy);
}

Console.WriteLine($"У читача активних видач: {reader.ActiveBookCopyIds.Count}");

// Спроба видати 6-ту книгу
var extraCopy = BookCopy.Create("BC-06", "978-0-123456-06");
try
{
    reader.BorrowBook(extraCopy);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[+] Інваріант спрацював: {ex.Message}");
    Console.WriteLine($"Чи видано 6-ту книгу: {extraCopy.IsIssued}"); // false — стан не змінився
}

Console.WriteLine();
Console.WriteLine("=== Додаткове завдання 3: Переходи станів (State Machine) ===");

// 1. Успішний життєвий цикл: Draft -> Confirmed
Order order = Order.Create("ORD-501", "CUST-99");
Console.WriteLine(order);

order.Confirm();
Console.WriteLine($"Після підтвердження: {order}");

// 2. Спроба недопустимого переходу: повторний Confirm для вже підтвердженого
TryDo("Повторний Confirm", () => order.Confirm());

// 3. Успішне скасування: Confirmed -> Cancelled
order.Cancel();
Console.WriteLine($"Після скасування: {order}");

// 4. Спроба воскресити скасоване замовлення: Cancelled -> Confirmed
TryDo("Підтвердження скасованого замовлення", () => order.Confirm());