namespace ReferenceDataApi.Dtos;

public sealed class CreateReferenceDataRequest
{
    public required string Label { get; init; }
    public int? DerivesFrom { get; init; }
    public bool Active { get; init; } = true;
}