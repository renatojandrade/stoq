namespace Stoq.ApiService.Hubs;

public interface IReceiptHubClient
{
    Task ReceiptCorrectionReady(Guid correctionId);

    Task ReceiptCorrectionFailed(Guid correctionId, string reason);
}
