using ReferenceDataApi.Domain;

namespace ReferenceDataApi.Dtos;

public sealed class ReferenceDataResponse
{
    public required int Id { get; init; }
    public required string Label { get; init; }
    public int? DerivesFrom { get; init; }
    public required bool Active { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required string ReferenceDataType { get; init; }

    public static ReferenceDataResponse From(string type, ReferenceDataItem item) => new()
    {
        ReferenceDataType = type,
        Id = item.Id,
        Label = item.Label,
        DerivesFrom = item.DerivesFrom,
        Active = item.Active,
        CreatedDate = item.CreatedDate
    };
}