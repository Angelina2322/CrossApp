using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Product> Products() =>
    [
        Product.Create("prod-001", "SKU-101", "Кабель UTP cat6", "м", 150),
        Product.Create("prod-002", "SKU-102", "Патч-корд 1м", "шт", 45),
        Product.Create("prod-003", "SKU-103", "Патч-корд 3м", "шт", 30),
        Product.Create("prod-004", "SKU-104", "Конектор RJ-45", "шт", 500),
        Product.Create("prod-005", "SKU-105", "Обтискний інструмент", "шт", 12),
        Product.Create("prod-006", "SKU-106", "Світч 8-портовий", "шт", 8),
        Product.Create("prod-007", "SKU-107", "Світч 16-портовий", "шт", 5),
        Product.Create("prod-008", "SKU-108", "Маршрутизатор Wi-Fi", "шт", 10),
        Product.Create("prod-009", "SKU-109", "Оптичний патчкорд SC", "шт", 60),
        Product.Create("prod-010", "SKU-110", "SFP-модуль 1Gb", "шт", 20),
        Product.Create("prod-011", "SKU-111", "Серверна шафа 9U", "шт", 3),
        Product.Create("prod-012", "SKU-112", "ДБЖ 650VA", "шт", 7),
        Product.Create("prod-013", "SKU-113", "Розетка кручена пара", "шт", 80),
        Product.Create("prod-014", "SKU-114", "Тестер мережевий кабельний", "шт", 6),
        Product.Create("prod-015", "SKU-115", "Стяжки кабельні 100мм", "пач", 40)
    ];
}