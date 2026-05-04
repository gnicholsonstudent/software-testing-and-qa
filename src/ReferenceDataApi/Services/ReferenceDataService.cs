using ReferenceDataApi.Domain;
using ReferenceDataApi.Dtos;
using ReferenceDataApi.Repositories;

namespace ReferenceDataApi.Services;

public sealed class ReferenceDataService : IReferenceDataService
{
    private readonly IReferenceDataRepository _repo;

    public ReferenceDataService(IReferenceDataRepository repo)
    {
        _repo = repo;
    }

    public ReferenceDataResponse? Get(string type, int id)
    {
        return _repo.TryGet(type, id, out var item) && item is not null
            ? ReferenceDataResponse.From(type, item)
            : null;
    }

    public ReferenceDataResponse Create(string type, CreateReferenceDataRequest request)
    {
        ValidateType(type);

        if (request.DerivesFrom is int parentId && !_repo.Exists(type, parentId))
            throw new InvalidOperationException($"DerivesFrom={parentId} not found for type '{type}'.");

        var created = _repo.Add(type, new ReferenceDataItem
        {
            Id = 0, // repo assigns
            Label = request.Label.Trim(),
            DerivesFrom = request.DerivesFrom,
            Active = request.Active,
            CreatedDate = DateTime.UtcNow
        });

        return ReferenceDataResponse.From(type, created);
    }

    public bool Update(string type, int id, UpdateReferenceDataRequest request, out string? error)
    {
        error = null;
        ValidateType(type);

        if (!_repo.TryGet(type, id, out var existing) || existing is null)
            return false;

        if (request.DerivesFrom == id)
        {
            error = "DerivesFrom cannot reference the item itself.";
            return false;
        }

        if (request.DerivesFrom is int parentId && !_repo.Exists(type, parentId))
        {
            error = $"DerivesFrom={parentId} not found for type '{type}'.";
            return false;
        }

        var updated = new ReferenceDataItem
        {
            Id = id,
            Label = request.Label.Trim(),
            DerivesFrom = request.DerivesFrom,
            Active = request.Active,
            CreatedDate = existing.CreatedDate // preserve
        };

        return _repo.Update(type, updated);
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
}