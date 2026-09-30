using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i];

            // Пропуск порожніх рядків та коментарів
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск рядка заголовків
            if (lineNumber == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {lineNumber}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Патерн властивостей: недостатня кількість колонок
            { Length: < 5 } => new ParseFailed($"очікую 5 колонок, отримав {parts.Length}"),

            // Константний патерн у списку: порожній SKU або назва
            [_, "", _, _, _] or [_, _, "", _, _] => new ParseFailed("SKU або назва порожні"),

            // Патерн списку з умовою when: перевірка валідності та знаку числа
            [_, _, _, _, var qty] when !int.TryParse(qty, out int q) || q < 0
                => new ParseFailed($"кількість '{qty}' не є невід'ємним цілим числом"),

            // Успішний збіг форми масиву з 5 колонок
            [var id, var sku, var name, var unit, var qty]
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty))),

            // Усі інші випадки (наприклад, зайві колонки)
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}