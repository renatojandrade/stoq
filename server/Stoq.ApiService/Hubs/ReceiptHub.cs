using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Stoq.ApiService.Hubs;

[Authorize]
public sealed class ReceiptHub : Hub<IReceiptHubClient>;
