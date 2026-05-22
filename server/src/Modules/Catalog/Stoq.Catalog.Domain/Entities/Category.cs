namespace Stoq.Catalog.Domain.Entities;

public sealed class Category
{
    // EF Core
    private Category()
    {
    }

    private Category(Guid id, string nameEn, string namePt)
    {
        Id = id;
        NameEn = nameEn;
        NamePt = namePt;
    }

    public Guid Id { get; private set; }

    public string NameEn { get; private set; } = string.Empty;

    public string NamePt { get; private set; } = string.Empty;

    public static Category Create(Guid id, string nameEn, string namePt) =>
        new(id, nameEn, namePt);
}
