using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<IEntityDto> Load(string path)
    {
        var items = new List<IEntityDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
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

        return new ImportResult<IEntityDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Товар: префікс 'P', далі 5 колонок
            ["P", _, "", _, _, _] or ["P", _, _, "", _, _]
                => new ParseFailed("Товар: SKU або назва порожні"),

            ["P", _, _, _, _, var qty] when !int.TryParse(qty, out int q) || q < 0
                => new ParseFailed($"Товар: кількість '{qty}' некоректна"),

            ["P", var id, var sku, var name, var unit, var qty]
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty))),

            // Склад: префікс 'W', код та локація не порожні
            ["W", _, "", _] or ["W", _, _, ""]
                => new ParseFailed("Склад: код або локація порожні"),

            ["W", var id, var code, var location]
                => new ParseOk(new WarehouseDto(id, code, location)),

            // Невідомий префікс або некоректна форма
            [var prefix, ..] when prefix != "P" && prefix != "W"
                => new ParseFailed($"Невідомий тип запису: '{prefix}'"),

            _ => new ParseFailed($"Некоректний формат рядка ({parts.Length} колонок)")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(IEntityDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}