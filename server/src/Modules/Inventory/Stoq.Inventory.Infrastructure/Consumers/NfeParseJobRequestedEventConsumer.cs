using MassTransit;
using Microsoft.Extensions.Logging;
using Stoq.Inventory.Application.Common.DTOs;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Domain.Entities;
using Stoq.Shared.Contracts.Events.Inventory;
using Stoq.Shared.Kernel.Services;

namespace Stoq.Inventory.Infrastructure.Consumers;

public sealed class NfeParseJobRequestedEventConsumer(
    IParseCorrectionsRepository repository,
    INfeReceiptParserService nfeParser,
    IHouseholdProductLookupService productLookup,
    IPublishEndpoint bus,
    IReceiptNotificationService notifier,
    ILogger<NfeParseJobRequestedEventConsumer> logger)
    : IConsumer<NfeParseJobRequestedEvent>
{
    public async Task Consume(ConsumeContext<NfeParseJobRequestedEvent> context)
    {
        NfeParseJobRequestedEvent evt = context.Message;

        var markedProcessing = await repository.MarkProcessingAsync(evt.CorrectionId, context.CancellationToken);
        if (!markedProcessing)
        {
            logger.LogWarning(
                "ParseCorrection {CorrectionId} not found or already completed — skipping.",
                evt.CorrectionId);
            return;
        }

        logger.LogInformation("Parsing NF-e for correction {CorrectionId}.", evt.CorrectionId);

        ParseReceiptResultDto parsed = await nfeParser.ParseAsync(evt.QrCodeUrl, context.CancellationToken);

        var items = new List<ParseCorrectionItem>(parsed.Items.Count);
        var eventItems = new List<NfeParsedItemData>(parsed.Items.Count);

        foreach (ParsedReceiptItemDto item in parsed.Items)
        {
            var correctionItemId = Guid.NewGuid();

            var matchedProductId = (Guid?)null;
            if (!string.IsNullOrWhiteSpace(item.Barcode))
            {
                matchedProductId = await productLookup
                    .FindHouseholdProductByBarcodeAsync(evt.HouseholdId, item.Barcode, context.CancellationToken);
            }

            items.Add(ParseCorrectionItem.Create(
                correctionItemId,
                evt.CorrectionId,
                item.ProductName,
                item.Quantity,
                item.Unit,
                item.Barcode,
                item.RetailerSku,
                item.UnitPrice,
                item.TotalPrice,
                item.Discount,
                "BRL"));

            eventItems.Add(new NfeParsedItemData(
                correctionItemId,
                item.ProductName,
                item.Unit,
                item.Quantity,
                item.Barcode,
                item.RetailerSku,
                item.UnitPrice,
                item.TotalPrice,
                item.Discount,
                Currency: "BRL",
                MatchedProductId: matchedProductId));
        }

        // Use ExecuteUpdateAsync (direct SQL) to avoid DbUpdateConcurrencyException caused by
        // EF Core's ValueGeneratedOnAdd behaviour on the Status property (set by HasDefaultValue).
        await repository.MarkParsedAsync(
            evt.CorrectionId,
            parsed.RetailerName,
            parsed.RetailerInfo?.Cnpj,
            parsed.RetailerInfo?.TradeName,
            parsed.RetailerInfo?.Street,
            parsed.RetailerInfo?.City,
            parsed.RetailerInfo?.State,
            parsed.RetailerInfo?.ZipCode,
            parsed.ReceiptNumber,
            parsed.EmittedAt,
            parsed.Items.Count,
            context.CancellationToken);

        // Insert freshly-constructed items (no prior tracking, INSERT-only SaveChanges).
        await repository.AddCorrectionItemsAsync(items, context.CancellationToken);

        await bus.Publish(
            new NfeParsedIntegrationEvent(
                evt.CorrectionId,
                evt.HouseholdId,
                parsed.RetailerName,
                parsed.RetailerInfo?.Cnpj,
                parsed.RetailerInfo?.TradeName,
                parsed.RetailerInfo?.Street,
                parsed.RetailerInfo?.City,
                parsed.RetailerInfo?.State,
                parsed.RetailerInfo?.ZipCode,
                parsed.Items.Count,
                eventItems,
                evt.InitiatedByUserId),
            context.CancellationToken);

        await notifier.NotifyCorrectionReadyAsync(evt.CorrectionId, evt.InitiatedByUserId, context.CancellationToken);

        logger.LogInformation(
            "NF-e parse complete for correction {CorrectionId} — {ItemCount} items.",
            evt.CorrectionId,
            parsed.Items.Count);
    }
}
