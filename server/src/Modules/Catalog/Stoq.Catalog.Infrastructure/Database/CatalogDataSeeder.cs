using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Stoq.Catalog.Domain.Constants;
using Stoq.Catalog.Infrastructure.Persistence;

namespace Stoq.Catalog.Infrastructure.Database;

internal sealed class CatalogDataSeeder(
    CatalogModuleDbContext context,
    IConfiguration configuration,
    ILogger<CatalogDataSeeder> logger)
{
    private const string _csvPathKey = "Catalog:SeedDataPath";

    // System user GUID used as CreatedBy/UpdatedBy for seeded data.
    private const string _systemUser = "00000000-0000-0000-0000-000000000000";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var hasReferenceProducts = await context.Products
            .AnyAsync(p => p.HouseholdId == null, ct);

        if (hasReferenceProducts)
        {
            logger.LogInformation("Reference product catalog is already seeded — skipping.");
            return;
        }

        var csvPath = configuration[_csvPathKey];
        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
        {
            logger.LogWarning(
                "Catalog seed file not found at '{Path}'. Set '{Key}' in configuration to enable seeding.", csvPath, _csvPathKey);
            return;
        }

        logger.LogInformation("Seeding reference product catalog from '{Path}'…", csvPath);

        await context.Database.OpenConnectionAsync(ct);
        var conn = (NpgsqlConnection)context.Database.GetDbConnection();

        await SeedFromCsvAsync(conn, csvPath, ct);
    }

    private async Task SeedFromCsvAsync(NpgsqlConnection conn, string csvPath, CancellationToken ct)
    {
        // Step 1 — temp staging table (dropped automatically at end of session).
        await using (NpgsqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TEMP TABLE _catalog_seed (
                    barcode  TEXT,
                    name     TEXT NOT NULL,
                    brand    TEXT,
                    category TEXT,
                    country  TEXT
                );
                """;
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // Step 2 — stream the CSV into the staging table via binary COPY.
        var rowsLoaded = 0;

        await using (NpgsqlBinaryImporter writer = await conn.BeginBinaryImportAsync(
            "COPY _catalog_seed (barcode, name, brand, category, country) FROM STDIN (FORMAT BINARY)", ct))
        {
            using var reader = new StreamReader(csvPath, System.Text.Encoding.UTF8);

            // Skip the header line.
            await reader.ReadLineAsync(ct);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                var cols = line.Split('\t');
                if (cols.Length < 5)
                {
                    continue;
                }

                var name = cols[1].Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var barcode = cols[0].Trim();
                var brand = cols[2].Trim();
                var category = cols[3].Trim();
                var country = cols[4].Trim();

                await writer.StartRowAsync(ct);

                // barcode — skip if empty or longer than 14 chars (invalid EAN/UPC).
                if (string.IsNullOrWhiteSpace(barcode) || barcode.Length > 14)
                {
                    await writer.WriteNullAsync(ct);
                }
                else
                {
                    await writer.WriteAsync(barcode, NpgsqlDbType.Text, ct);
                }

                // name — truncate to 200.
                await writer.WriteAsync(name.Length > 200 ? name[..200] : name, NpgsqlDbType.Text, ct);

                // brand — nullable, truncate to 200.
                if (string.IsNullOrWhiteSpace(brand))
                {
                    await writer.WriteNullAsync(ct);
                }
                else
                {
                    await writer.WriteAsync(brand.Length > 200 ? brand[..200] : brand, NpgsqlDbType.Text, ct);
                }

                // category — nullable in staging (defaulted to Uncategorized in INSERT).
                if (string.IsNullOrWhiteSpace(category))
                {
                    await writer.WriteNullAsync(ct);
                }
                else
                {
                    await writer.WriteAsync(category.Length > 100 ? category[..100] : category, NpgsqlDbType.Text, ct);
                }

                // country — nullable (NULL = globally available).
                if (string.IsNullOrWhiteSpace(country))
                {
                    await writer.WriteNullAsync(ct);
                }
                else
                {
                    await writer.WriteAsync(country, NpgsqlDbType.Text, ct);
                }

                rowsLoaded++;
            }

            await writer.CompleteAsync(ct);
        }

        logger.LogInformation("Loaded {Rows} rows into staging table.", rowsLoaded);

        // Step 3a — Insert unique global brands from staging into Brands table.
        await using (NpgsqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = $"""
                INSERT INTO catalog."Brands" ("Id", "Name", "HouseholdId", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy")
                SELECT
                    gen_random_uuid(),
                    brand,
                    NULL,
                    NOW(),
                    '{_systemUser}'::uuid,
                    NOW(),
                    '{_systemUser}'::uuid
                FROM (
                    SELECT DISTINCT TRIM(brand) AS brand
                    FROM _catalog_seed
                    WHERE TRIM(brand) IS NOT NULL AND TRIM(brand) != ''
                ) distinct_brands
                ON CONFLICT ("Name") WHERE "HouseholdId" IS NULL DO NOTHING;
                """;
            cmd.CommandTimeout = 600;
            var brandsInserted = await cmd.ExecuteNonQueryAsync(ct);
            logger.LogInformation("Seeded {Count} global brands.", brandsInserted);
        }

        // Step 3b — INSERT from staging into Products.
        // ON CONFLICT targets the partial unique index on Barcode (WHERE Barcode IS NOT NULL).
        int inserted;

        await using (NpgsqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = $"""
                INSERT INTO catalog."Products" (
                    "Id", "HouseholdId", "Name", "CategoryId", "BrandId", "UnitId",
                    "MinQuantity", "ImageUrl", "Barcode", "Country",
                    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
                )
                SELECT
                    gen_random_uuid(),
                    NULL,
                    s.name,
                    COALESCE(c."Id", '{WellKnownIds.Categories.Uncategorized}'::uuid),
                    b."Id",
                    '{WellKnownIds.Units.UnitsId}'::uuid,
                    0,
                    NULL,
                    s.barcode,
                    NULLIF(s.country, ''),
                    NOW(),
                    '{_systemUser}'::uuid,
                    NOW(),
                    '{_systemUser}'::uuid
                FROM _catalog_seed s
                LEFT JOIN catalog."Brands" b ON b."Name" = NULLIF(TRIM(s.brand), '') AND b."HouseholdId" IS NULL
                LEFT JOIN catalog."Categories" c ON c."NameEn" = NULLIF(TRIM(s.category), '')
                ON CONFLICT ("Barcode") WHERE "Barcode" IS NOT NULL DO NOTHING;
                """;

            // Allow up to 10 minutes for a large initial seed.
            cmd.CommandTimeout = 600;

            inserted = await cmd.ExecuteNonQueryAsync(ct);
        }

        logger.LogInformation("Seeded {Count} reference products into the catalog.", inserted);
    }
}
