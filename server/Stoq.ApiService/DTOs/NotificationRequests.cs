using System.Text.Json.Serialization;

namespace Stoq.ApiService.DTOs;

public sealed record RegisterPushTokenRequest(
    [property: JsonRequired] string Token);
