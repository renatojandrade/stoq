using System.Diagnostics;

namespace Stoq.Shared.Kernel.Telemetry;

public static class StoqActivitySource
{
    private static readonly ActivitySource _source = new("Stoq.Domain", "1.0.0");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal) =>
        _source.StartActivity(name, kind);
}
