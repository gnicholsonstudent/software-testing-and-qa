namespace ReferenceDataApi.Contracts;

public sealed record ReferenceDataDto(
    int Id,
    string Label,
    int? DerivesFrom,
    bool Active,
    DateTime CreatedDate
);