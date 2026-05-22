namespace Stoq.Catalog.Domain.Entities;

public sealed class UnitOfMeasure
{
    // EF Core
    private UnitOfMeasure()
    {
    }

    private UnitOfMeasure(Guid id, string code, string nameEn, string namePt)
    {
        Id = id;
        Code = code;
        NameEn = nameEn;
        NamePt = namePt;
    }

    public Guid Id { get; private set; }

    /// <summary>Canonical code stored as snapshot in InventoryItem.Unit (e.g. "Units", "Grams").</summary>
    public string Code { get; private set; } = string.Empty;

    public string NameEn { get; private set; } = string.Empty;

    public string NamePt { get; private set; } = string.Empty;

    public static UnitOfMeasure Create(Guid id, string code, string nameEn, string namePt) =>
        new(id, code, nameEn, namePt);
}
