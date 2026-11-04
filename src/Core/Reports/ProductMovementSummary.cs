namespace Core.Reports;

// Зведений DTO: товар + сумарні показники його переміщень
public sealed record ProductMovementSummary(
    string Sku,
    string Name,
    int CurrentStock,
    int MovementsCount,
    int TotalMovedQuantity
);