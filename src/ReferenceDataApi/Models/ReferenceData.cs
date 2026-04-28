namespace ReferenceDataApi.Models;

public sealed class ReferenceData
{
    public int Id { get; init; }
    public string Label { get; set; } = string.Empty;
    public int? DerivesFrom { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; init; }
}