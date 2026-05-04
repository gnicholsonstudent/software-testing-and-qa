using System.Collections.Concurrent;
using System.Threading;
using ReferenceDataApi.Domain;

namespace ReferenceDataApi.Repositories;

public sealed class InMemoryReferenceDataRepository : IReferenceDataRepository
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ReferenceDataItem>> _store =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, int> _idCounters =
        new(StringComparer.OrdinalIgnoreCase);

    public InMemoryReferenceDataRepository()
    {
        Seed();
    }

    public bool TryGet(string type, int id, out ReferenceDataItem? item)
    {
        item = null;
        if (!_store.TryGetValue(NormaliseType(type), out var bucket))
            return false;

        return bucket.TryGetValue(id, out item);
    }

    public bool Exists(string type, int id)
        => _store.TryGetValue(NormaliseType(type), out var bucket) && bucket.ContainsKey(id);

    public ReferenceDataItem Add(string type, ReferenceDataItem item)
    {
        var t = NormaliseType(type);
        var bucket = _store.GetOrAdd(t, _ => new ConcurrentDictionary<int, ReferenceDataItem>());

        var nextId = NextId(t);

        var created = new ReferenceDataItem
        {
            Id = nextId,
            Label = item.Label,
            DerivesFrom = item.DerivesFrom,
            Active = item.Active,
            CreatedDate = item.CreatedDate
        };

        bucket[nextId] = created;
        return created;
    }

    public bool Update(string type, ReferenceDataItem item)
    {
        var t = NormaliseType(type);
        if (!_store.TryGetValue(t, out var bucket))
            return false;

        if (!bucket.TryGetValue(item.Id, out var existing))
            return false;

        // Preserve CreatedDate (immutable)
        existing.Label = item.Label;
        existing.DerivesFrom = item.DerivesFrom;
        existing.Active = item.Active;

        return true;
    }

    public bool Delete(string type, int id)
    {
        var t = NormaliseType(type);
        if (!_store.TryGetValue(t, out var bucket))
            return false;

        return bucket.TryRemove(id, out _);
    }

    private int NextId(string type)
    {
        // Counters start at existing max seeded id.
        return _idCounters.AddOrUpdate(
            type,
            addValueFactory: _ => 1,
            updateValueFactory: (_, current) => current + 1
        );
    }

    private static string NormaliseType(string type) => type.Trim();

    private void Seed()
    {
        // Seed buckets
        var location = _store.GetOrAdd("location", _ => new ConcurrentDictionary<int, ReferenceDataItem>());
        var commodity = _store.GetOrAdd("commodity", _ => new ConcurrentDictionary<int, ReferenceDataItem>());
        var product = _store.GetOrAdd("product", _ => new ConcurrentDictionary<int, ReferenceDataItem>());

        // Seed data
        location[1] = new ReferenceDataItem { Id = 1, Label = "United Kingdom", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-30) };
        location[2] = new ReferenceDataItem { Id = 2, Label = "London", DerivesFrom = 1, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-20) };

        commodity[1] = new ReferenceDataItem { Id = 1, Label = "Aluminium", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-10) };
        commodity[2] = new ReferenceDataItem { Id = 2, Label = "Copper", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-8) };

        product[1] = new ReferenceDataItem { Id = 1, Label = "HRC Steel", DerivesFrom = null, Active = true, CreatedDate = DateTime.UtcNow.AddDays(-15) };

        // Set counters to max ids (so new inserts continue after seeded ids)
        _idCounters["location"] = location.Keys.DefaultIfEmpty(0).Max();
        _idCounters["commodity"] = commodity.Keys.DefaultIfEmpty(0).Max();
        _idCounters["product"] = product.Keys.DefaultIfEmpty(0).Max();
    }
}