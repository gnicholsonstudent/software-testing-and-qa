namespace ReferenceDataApi.Security;

public sealed class ApiKeyOptions
{
    public string? HeaderName { get; set; } = "X-API-KEY";
    public List<ApiKeyRecord> Keys { get; set; } = new();
}