using System.Collections.Generic;
using Core.Dto;

namespace Core.Domain;

// Запис із успішно створеними сутностями та списком виявлених порушень
public sealed record DomainImportResult(
    IReadOnlyList<Product> Products,
    IReadOnlyList<DomainRowError> Errors
);

// Структура для фіксації рядка з порушенням інваріанту
public sealed record DomainRowError(
    ProductDto RawDto,
    string ErrorMessage,
    string ExceptionType
);