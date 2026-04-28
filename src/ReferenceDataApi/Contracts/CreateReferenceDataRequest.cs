using System.ComponentModel.DataAnnotations;

namespace ReferenceDataApi.Contracts;

public sealed class CreateReferenceDataRequest
{
    [Required]
    [MinLength(1)]
    public string Label { get; set; } = string.Empty;

    public int? DerivesFrom { get; set; }

    public bool Active { get; set; } = true;
}