using ReferenceDataApi.Contracts;
using ReferenceDataApi.Models;
using ReferenceDataApi.Repositories;

namespace ReferenceDataApi.Services;

public sealed class ReferenceDataService : IReferenceDataService
{
    private readonly IReferenceDataRepository _repo;

    public ReferenceDataService(IReferenceDataRepository repo)
    {
        _repo = repo;
    }

    public ReferenceDataDto? Get(string type, int id)
    {
        var item = _repo.Get(type, id);
        return item is null ? null : ToDto(item);
    }

    public ReferenceDataDto Create(string type, CreateReferenceDataRequest request)
    {
        ValidateType(type);

        if (request.DerivesFrom is int parentId && !_repo.Exists(type, parentId))
            throw new InvalidOperationException($"DerivesFrom id {parentId} does not exist in type '{type}'.");

        var newId = _repo.NextId(type);

        var entity = new ReferenceData
        {
            Id = newId,
            Label = request.Label.Trim(),
            DerivesFrom = request.DerivesFrom,
            Active = request.Active,
            CreatedDate = DateTime.UtcNow
        };

        var created = _repo.Create(type, entity);
        return ToDto(created);
    }

    public ReferenceDataDto? Update(string type, int id, UpdateReferenceDataRequest request)
    {
        ValidateType(type);

        if (request.DerivesFrom == id)
            throw new InvalidOperationException("DerivesFrom cannot reference the same item (self-reference).");

        if (request.DerivesFrom is int parentId && !_repo.Exists(type, parentId))
            throw new InvalidOperationException($"DerivesFrom id {parentId} does not exist in type '{type}'.");

        var updated = _repo.Update(type, id, existing =>
        {
            // Preserve CreatedDate and Id
            return new ReferenceData
            {
                Id = existing.Id,
                CreatedDate = existing.CreatedDate,
                Label = request.Label.Trim(),
                DerivesFrom = request.DerivesFrom,
                Active = request.Active
            };
        });

        return updated is null ? null : ToDto(updated);
    }

    public bool Delete(string type, int id)
    {
        ValidateType(type);
        return _repo.Delete(type, id);
    }

    private static void ValidateType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("referenceDataType must be provided.", nameof(type));
    }

    private static ReferenceDataDto ToDto(ReferenceData item)
        => new(item.Id, item.Label, item.DerivesFrom, item.Active, item.CreatedDate);
}
