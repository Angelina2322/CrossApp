using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class FileCatalogStoreTests : IDisposable
{
    private readonly string _tempDir;

    public FileCatalogStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "CrossAppTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void Add_And_Reload_PreservesCyrillicAndData()
    {
        var filePath = Path.Combine(_tempDir, "items.json");
        var store = new FileCatalogStore(filePath);

        store.Add(Product.Create("id-1", "A-100", "Цемент М400", "мішок", 40));

        // Перевіряємо, що кирилиця не екранована як \u0426
        var rawJson = File.ReadAllText(filePath);
        Assert.Contains("Цемент М400", rawJson);
        Assert.DoesNotContain("\\u0426", rawJson);

        // Перевіряємо завантаження новим екземпляром сховища
        var reloadedStore = new FileCatalogStore(filePath);
        var loaded = reloadedStore.GetById("id-1");
        Assert.NotNull(loaded);
        Assert.Equal("Цемент М400", loaded.Name);
        Assert.Equal(40, loaded.Quantity);
    }

    [Fact]
    public void CorruptedJson_ThrowsInvalidDataException()
    {
        var filePath = Path.Combine(_tempDir, "broken.json");
        File.WriteAllText(filePath, "{ invalid json [");

        var store = new FileCatalogStore(filePath);

        Assert.Throws<InvalidDataException>(() => store.List());
    }

    [Fact]
    public void StorageOptions_ResolveFullPath_CreatesDirectoryAndResolvesRelativePath()
    {
        var options = new StorageOptions { DataPath = "subdata", FileName = "catalog.json" };
        var resolved = options.ResolveFullPath(_tempDir);

        Assert.True(Directory.Exists(Path.Combine(_tempDir, "subdata")));
        Assert.Equal(Path.GetFullPath(Path.Combine(_tempDir, "subdata", "catalog.json")), resolved);
    }
}