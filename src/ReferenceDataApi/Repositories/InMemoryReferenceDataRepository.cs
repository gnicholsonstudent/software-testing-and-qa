using System.Collections.Concurrent;
using ReferenceDataApi.Models;

namespace ReferenceDataApi.Repositories;

public sealed class InMemoryReferenceDataRepository : IReferenceDataRepository
{
    // type -> (id -> entity)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ReferenceData>> _store
        = new(StringComparer.OrdinalIgnoreCase);

    // type -> id counter
    private readonly ConcurrentDictionary<string, int> _counters
        = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryReferenceDataRepository()
    {
        Seed();
    }

    public ReferenceData? Get(string type, int id)
        => GetBucket(type).TryGetValue(id, out var item) ? item : null;

    public bool Exists(string type, int id)
        => GetBucket(type).ContainsKey(id);

    public int NextId(string type)
        => _counters.AddOrUpdate(NormalizeType(type), 1, (_, current) => current + 1);

    public ReferenceData Create(string type, ReferenceData item)
    {
        var bucket = GetBucket(type);
        if (!bucket.TryAdd(item.Id, item))
            throw new InvalidOperationException($"Item with id {item.Id} already exists in type '{type}'.");

        return item;
    }

    public ReferenceData? Update(string type, int id, Func<ReferenceData, ReferenceData> update)
    {
        var bucket = GetBucket(type);

        while (true)
        {
            if (!bucket.TryGetValue(id, out var existing))
                return null;

            var updated = update(existing);

            if (bucket.TryUpdate(id, updated, existing))
                return updated;
        }
    }

    public bool Delete(string type, int id)
        => GetBucket(type).TryRemove(id, out _);

    private ConcurrentDictionary<int, ReferenceData> GetBucket(string type)
        => _store.GetOrAdd(NormalizeType(type), _ => new ConcurrentDictionary<int, ReferenceData>());

    private static string NormalizeType(string type)
        => type.Trim();

    private void Seed()
    {
        // Seed types
        SeedType("location", new[]
        {
            new ReferenceData { Id = 1, Label = "London", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-30) },
            new ReferenceData { Id = 2, Label = "Manchester", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-25) },
            new ReferenceData { Id = 3, Label = "Canary Wharf", DerivesFrom = 1, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-10) }
        });

        SeedType("commodity", new[]
        {
            new ReferenceData { Id = 1, Label = "Metals", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-60) },
            new ReferenceData { Id = 2, Label = "Copper", DerivesFrom = 1, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-40) },
            new ReferenceData { Id = 3, Label = "Aluminium", DerivesFrom = 1, Active = false, CreatedDate = DateTime.UtcNow.AddDays(-20) }
        });

        SeedType("product", new[]
        {
            new ReferenceData { Id = 1, Label = "Laptop", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-15) },
            new ReferenceData { Id = 2, Label = "Docking Station", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-12) }
        });
    }

    private void SeedType(string type, IEnumerable<ReferenceData> items)
    {
        var bucket = GetBucket(type);

        var maxId = 0;
        foreach (var item in items)
        {
            bucket[item.Id] = item;
            if (item.Id > maxId) maxId = item.Id;
        }

        _counters[NormalizeType(type)] = maxId;
    }
}