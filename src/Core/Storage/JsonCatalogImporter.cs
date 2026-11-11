using System.Text.Json;
using Core.Abstractions;
using  Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
{
    ct.ThrowIfCancellationRequested(); // Якщо токен скасовано заздалегідь — виходимо одразу

    await using var stream = new FileStream(
        path,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        bufferSize: 4096,
        useAsync: true);

        // Десеріалізуємо DTO-об'єкти
        var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<Product>(dtos.Count);

        for (var i = 0; i < dtos.Count; i++)
        {
            // Перевіряємо скасування перед обробкою кожного елемента
            ct.ThrowIfCancellationRequested();

            try
            {
                result.Add(Product.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                // Загортаємо помилку валідації сутності у доменний виняток із зазначенням номера рядка
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }
}