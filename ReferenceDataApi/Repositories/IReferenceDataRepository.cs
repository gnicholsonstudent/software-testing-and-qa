using ReferenceDataApi.Models;

namespace ReferenceDataApi.Repositories;

public interface IReferenceDataRepository
{
    ReferenceData? Get(string type, int id);
    ReferenceData Create(string type, ReferenceData item);
    ReferenceData? Update(string type, int id, Func<ReferenceData, ReferenceData> update);
    bool Delete(string type, int id);

    bool Exists(string type, int id);
    int NextId(string type);
}