namespace ReferenceDataApi.Security;

public sealed class ApiKeyRecord
{
    public required string Key { get; set; }
    public required string Role { get; set; } // "Admin" or "Reader"
}