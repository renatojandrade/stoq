using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Stoq.Catalog.Infrastructure.Persistence;

namespace Stoq.Catalog.Infrastructure.Database;

internal sealed class CatalogDataSeeder(
    CatalogModuleDbContext context,
    IConfiguration configuration,
    ILogger<CatalogDataSeeder> logger)
{
    private const string _csvPathKey = "Catalog:SeedDataPath";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var hasReferenceProducts = await context.Products
            .AnyAsync(p => p.HouseholdId == null, ct)
            .ConfigureAwait(false);

        if (hasReferenceProducts)
        {
            logger.LogInformation("Reference product catalog is already seeded — skipping.");
            return;
        }

        var csvPath = configuration[_csvPathKey];
        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
        {
            logger.LogWarning(
                "Catalog seed file not found at '{Path}'. Set '{Key}' in configuration to enable seeding.",
                csvPath, _csvPathKey);
            return;
        }

        logger.LogInformation("Seeding reference product catalog from '{Path}'…", csvPath);

        await context.Database.OpenConnectionAsync(ct).ConfigureAwait(false);
        var conn = (NpgsqlConnection)context.Database.GetDbConnection();

        await SeedFromCsvAsync(conn, csvPath, ct).ConfigureAwait(false);
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
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        // Step 2 — stream the CSV into the staging table via binary COPY.
        var rowsLoaded = 0;

        await using (NpgsqlBinaryImporter writer = await conn.BeginBinaryImportAsync(
            "COPY _catalog_seed (barcode, name, brand, category, country) FROM STDIN (FORMAT BINARY)", ct)
            .ConfigureAwait(false))
        {
            using var reader = new StreamReader(csvPath, System.Text.Encoding.UTF8);

            // Skip the header line.
            await reader.ReadLineAsync(ct).ConfigureAwait(false);

            while (await reader.ReadLineAsync(ct).ConfigureAwait(false) is { } line)
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

                var barcode  = cols[0].Trim();
                var brand    = cols[2].Trim();
                var category = cols[3].Trim();
                var country  = cols[4].Trim();

                await writer.StartRowAsync(ct).ConfigureAwait(false);

                // barcode — skip if empty or longer than 14 chars (invalid EAN/UPC).
                if (string.IsNullOrWhiteSpace(barcode) || barcode.Length > 14)
                {
                    await writer.WriteNullAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteAsync(barcode, NpgsqlDbType.Text, ct).ConfigureAwait(false);
                }

                // name — truncate to 200.
                await writer.WriteAsync(
                    name.Length > 200 ? name[..200] : name,
                    NpgsqlDbType.Text, ct).ConfigureAwait(false);

                // brand — nullable, truncate to 200.
                if (string.IsNullOrWhiteSpace(brand))
                {
                    await writer.WriteNullAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteAsync(
                        brand.Length > 200 ? brand[..200] : brand,
                        NpgsqlDbType.Text, ct).ConfigureAwait(false);
                }

                // category — nullable in staging (defaulted in INSERT).
                if (string.IsNullOrWhiteSpace(category))
                {
                    await writer.WriteNullAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteAsync(
                        category.Length > 100 ? category[..100] : category,
                        NpgsqlDbType.Text, ct).ConfigureAwait(false);
                }

                // country — nullable (NULL = globally available).
                if (string.IsNullOrWhiteSpace(country))
                {
                    await writer.WriteNullAsync(ct).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteAsync(country, NpgsqlDbType.Text, ct).ConfigureAwait(false);
                }

                rowsLoaded++;
            }

            await writer.CompleteAsync(ct).ConfigureAwait(false);
        }

        logger.LogInformation("Loaded {Rows} rows into staging table.", rowsLoaded);

        // Step 3 — INSERT from staging into Products.
        // ON CONFLICT targets the partial unique index on Barcode (WHERE Barcode IS NOT NULL).
        // Rows without a barcode (null) are always inserted — no conflict is possible.
        int inserted;

        await using (NpgsqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
                INSERT INTO catalog."Products" (
                    "Id", "HouseholdId", "Name", "Brand", "Category",
                    "Unit", "MinQuantity", "ImageUrl", "Barcode", "Country",
                    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy"
                )
                SELECT
                    gen_random_uuid(),
                    NULL,
                    name,
                    NULLIF(brand, ''),
                    COALESCE(NULLIF(category, ''), 'Uncategorized'),
                    'Units',
                    1,
                    NULL,
                    barcode,
                    NULLIF(country, ''),
                    NOW(),
                    '00000000-0000-0000-0000-000000000000'::uuid,
                    NOW(),
                    '00000000-0000-0000-0000-000000000000'::uuid
                FROM _catalog_seed
                ON CONFLICT ("Barcode") WHERE "Barcode" IS NOT NULL DO NOTHING;
                """;

            // Allow up to 10 minutes for a large initial seed.
            cmd.CommandTimeout = 600;

            inserted = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        logger.LogInformation("Seeded {Count} reference products into the catalog.", inserted);
    }
}
