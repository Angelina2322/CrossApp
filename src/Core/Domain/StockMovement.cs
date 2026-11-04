namespace Core.Domain;

public sealed record StockMovement(string Id, string Sku, int Quantity, string Destination);