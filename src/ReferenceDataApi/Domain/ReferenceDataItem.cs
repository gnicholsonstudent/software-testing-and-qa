namespace ReferenceDataApi.Domain;

public sealed class ReferenceDataItem
{
    public required int Id { get; init; }
    public required string Label { get; set; }
    public int? DerivesFrom { get; set; }
    public required bool Active { get; set; }
    public required DateTime CreatedDate { get; init; }
}