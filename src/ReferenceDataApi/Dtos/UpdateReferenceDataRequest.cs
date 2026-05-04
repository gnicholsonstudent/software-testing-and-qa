namespace ReferenceDataApi.Dtos;

public sealed class UpdateReferenceDataRequest
{
    public required string Label { get; init; }
    public int? DerivesFrom { get; init; }
    public required bool Active { get; init; }
}