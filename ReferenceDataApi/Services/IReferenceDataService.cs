using ReferenceDataApi.Contracts;

namespace ReferenceDataApi.Services;

public interface IReferenceDataService
{
    ReferenceDataDto? Get(string type, int id);
    ReferenceDataDto Create(string type, CreateReferenceDataRequest request);
    ReferenceDataDto? Update(string type, int id, UpdateReferenceDataRequest request);
    bool Delete(string type, int id);
}
