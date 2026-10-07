namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public bool IsIssued { get; private set; }

    private BookCopy(string id, string isbn, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        IsIssued = isIssued;
    }

    public static BookCopy Create(string id, string isbn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim(), false);
    }

    public void MarkAsIssued()
    {
        if (IsIssued)
            throw new InvalidOperationException($"Примірник {Id} уже виданий.");
        IsIssued = true;
    }
}