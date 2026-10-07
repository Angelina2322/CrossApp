using System;
using System.Collections.Generic;
using Core.Dto;

namespace Core.Domain;

public static class ProductDomainImporter
{
    // Додано <ProductDto> до ImportResult
    public static DomainImportResult ProcessImportResult(ImportResult<ProductDto> importResult)
    {
        var validProducts = new List<Product>();
        var errors = new List<DomainRowError>();

        // Якщо у вашому ImportResult властивість називається не Items, а Data чи Rows — підставте її
        foreach (var dto in importResult.Items)
        {
            try
            {
                var product = Product.FromDto(dto);
                validProducts.Add(product);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                errors.Add(new DomainRowError(
                    dto,
                    ex.Message,
                    ex.GetType().Name
                ));
            }
        }

        return new DomainImportResult(
            validProducts.AsReadOnly(),
            errors.AsReadOnly()
        );
    }
}