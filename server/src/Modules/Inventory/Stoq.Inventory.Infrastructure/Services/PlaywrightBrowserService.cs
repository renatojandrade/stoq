using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Stoq.Inventory.Infrastructure.Services;

/// <summary>
/// Singleton that holds one reusable headless Chromium instance.
/// Browser creation is deferred until the first request.
/// </summary>
public sealed class PlaywrightBrowserService(ILogger<PlaywrightBrowserService> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    /// <summary>Called once at startup to install browsers and launch the process eagerly.</summary>
    public Task WarmUpAsync(CancellationToken ct) => GetBrowserAsync(ct);

    public async Task<string> GetRenderedHtmlAsync(string url, CancellationToken ct)
    {
        IBrowser browser = await GetBrowserAsync(ct);

        await using IBrowserContext context = await browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        try
        {
            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000,
            });

            return await page.ContentAsync();
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Playwright timed out waiting for NetworkIdle on {Url} — using partially rendered content.", url);
            return await page.ContentAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
        _lock.Dispose();
    }

    private async Task<IBrowser> GetBrowserAsync(CancellationToken ct)
    {
        if (_browser is not null)
        {
            return _browser;
        }

        await _lock.WaitAsync(ct);
        try
        {
            if (_browser is not null)
            {
                return _browser;
            }

            EnsureBrowsersInstalled();

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });

            logger.LogInformation("Playwright Chromium browser started.");
            return _browser;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to launch Playwright Chromium browser.");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureBrowsersInstalled()
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            logger.LogWarning("Playwright browser install exited with code {Code}.", exitCode);
        }
    }
}
