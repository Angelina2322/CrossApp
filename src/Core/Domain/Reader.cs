using System.Collections.Generic;

namespace Core.Domain;

public sealed class Reader
{
    private const int MaxActiveLoansLimit = 5;
    private readonly List<string> _activeBookCopyIds = [];

    public string Id { get; }
    public string FullName { get; }
    public IReadOnlyList<string> ActiveBookCopyIds => _activeBookCopyIds.AsReadOnly();

    private Reader(string id, string fullName)
    {
        Id = id;
        FullName = fullName;
    }

    public static Reader Create(string id, string fullName)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("ПІБ обов'язкове", nameof(fullName));

        return new Reader(id.Trim(), fullName.Trim());
    }

    // Інваріант, що охоплює дві сутності
    public void BorrowBook(BookCopy copy)
    {
        if (copy == null)
            throw new ArgumentNullException(nameof(copy));

        // 1. Перевірка правила, що стосується ліміту читача
        if (_activeBookCopyIds.Count >= MaxActiveLoansLimit)
            throw new InvalidOperationException(
                $"Читач {FullName} ({Id}) досяг ліміту: не можна мати більше {MaxActiveLoansLimit} відкритих видач.");

        // 2. Зміна стану примірника (перевірить власний інваріант IsIssued)
        copy.MarkAsIssued();

        // 3. Зміна стану читача
        _activeBookCopyIds.Add(copy.Id);
    }
}