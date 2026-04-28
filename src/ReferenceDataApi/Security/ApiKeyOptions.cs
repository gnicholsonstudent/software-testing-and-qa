namespace ReferenceDataApi.Security;

public sealed class ApiKeyOptions
{
    public string HeaderName { get; set; } = "X-API-KEY";
    public List<ApiKeyRecord> Keys { get; set; } = new();

    public sealed class ApiKeyRecord
    {
        public string Key { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin" or "Reader"
    }
}