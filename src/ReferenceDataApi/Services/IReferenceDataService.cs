using ReferenceDataApi.Dtos;

namespace ReferenceDataApi.Services;

public interface IReferenceDataService
{
    ReferenceDataResponse? Get(string type, int id);

    ReferenceDataResponse Create(string type, CreateReferenceDataRequest request);

    bool Update(string type, int id, UpdateReferenceDataRequest request, out string? error);

    bool Delete(string type, int id);
}