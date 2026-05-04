using ReferenceDataApi.Domain;

namespace ReferenceDataApi.Repositories;

public interface IReferenceDataRepository
{
    bool TryGet(string type, int id, out ReferenceDataItem? item);
    ReferenceDataItem Add(string type, ReferenceDataItem item);
    bool Update(string type, ReferenceDataItem item);
    bool Delete(string type, int id);
    bool Exists(string type, int id);
}