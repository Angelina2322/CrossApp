using System.Text.Json;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class CatalogTests
{
    // 1. Інваріант: відмова при некоректних даних та збереження початкового стану
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var product = service.Add("SKU-1", "Кабель", "м", 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(product.Id, -5));
        Assert.Equal(10, store.GetById(product.Id)!.Quantity);
    }

    // 2. Перевірка події: спрацьовує рівно один раз із правильним типом
    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    // 3. Звіт на кількох наборах параметрів ([Theory] + [InlineData])
    [Theory]
    [InlineData(10, 2)] // SKU-LOW (5) та SKU-ZERO (0) нижче 10
    [InlineData(1, 1)]  // Тільки SKU-ZERO (0) нижче 1
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        var result = ReportService.LowStock(TestData.Items(), threshold);
        Assert.Equal(expected, result.Count);
    }

    // 4. Детермінованість сортування топ-N
    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        var top = ReportService.TopByQuantity(TestData.Items(), 2);
        var skus = top.Select(x => x.Sku).ToArray();

        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, skus);
    }

    // 5. Кооперативне скасування: якщо токен уже скасовано, викидається OperationCanceledException
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.ImportAsync(new JsonCatalogImporter(), tempFile, cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    // 6. Тест на некоректні дані та викидання DomainException (з очищенням тимчасового файлу)
    [Fact]
    public async Task ImportAsync_InvalidData_ThrowsDomainException_AndCleansUpFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            // JSON валідний за структурою, але порожнє Name порушує валідацію в Product.Create
            var invalidJson = "[{\"Id\": \"p-1\", \"Sku\": \"SKU-ERR\", \"Name\": \"\", \"Unit\": \"шт\", \"Quantity\": 10}]";
            await File.WriteAllTextAsync(tempFile, invalidJson);

            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAsync<DomainException>(() =>
                service.ImportAsync(new JsonCatalogImporter(), tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
    [Fact]
public async Task ImportAsync_LinkedToken_WhenTimeoutExpires_CancelsByTimeout()
{
    var tempFile = Path.GetTempFileName();
    try
    {
        // 1. Токен користувача (ручний сигнал)
        using var userCts = new CancellationTokenSource();

        // 2. Токен таймауту на 20 мілісекунд
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));

        // 3. Зв'язане джерело токенів
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        // Чекаємо трохи довше за таймаут, щоб гарантовано спрацював саме він
        await Task.Delay(50);

        var service = new CatalogService(new InMemoryCatalogStore());

        // Перевіряємо, що кидається OperationCanceledException
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ImportAsync(new JsonCatalogImporter(), tempFile, linkedCts.Token));

        // ДОКАЗ: спрацював саме таймаут, а користувацький токен залишився нескасованим
        Assert.True(timeoutCts.IsCancellationRequested, "Таймаут мав спрацювати.");
        Assert.False(userCts.IsCancellationRequested, "Користувач не ініціював скасування.");
        Assert.True(linkedCts.IsCancellationRequested, "Зв'язаний токен має бути в стані скасування.");
    }
    finally
    {
        if (File.Exists(tempFile)) File.Delete(tempFile);
    }
}
}