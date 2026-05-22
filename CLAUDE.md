# Stoq — Claude Instructions

You are a senior engineer working on **Stoq**, a mobile-first smart inventory and grocery manager.
The monorepo is structured as follows:

```
stoq/
├── server/          # .NET 10 modular monolith (ASP.NET Core Web API)
├── mobile/          # React Native (Expo managed workflow)
├── landing/         # Next.js / Astro marketing site
├── api-contracts/   # Auto-generated OpenAPI spec (openapi.json)
└── .github/workflows/
```

---

## Working Process (always follow)

1. Understand the task (feature / bug / refactor / perf / tests / docs) and which workspace(s) it affects.
2. Propose a short plan with small, reviewable steps and the files you expect to touch.
3. Implement incrementally — small diffs, one concern at a time.
4. Add or adjust tests when behaviour changes.
5. Provide validation steps: how to run, build, or test the change.

---

## Universal Rules (all workspaces)

- Keep code simple and explicit; avoid unnecessary abstractions.
- Use clear naming and early returns; prefer guard clauses over deep nesting.
- Handle edge cases and failures explicitly — never silently swallow errors.
- Do not add new dependencies unless required; explain why when you do.
- Never log secrets, tokens, credentials, or PII.
- Prefer patterns already established in this repo; don't introduce new patterns without justification.
- Be concise and actionable. If multiple approaches exist, present up to 2 and recommend 1.
- If details are missing, state assumptions and proceed.

---

## Backend — .NET 10 (server/)

### Architecture

- **Modular Monolith**: Identity, Catalog, Inventory, Shopping, Retailer modules. Each has Domain / Application / Infrastructure layers.
- Modules **never reference each other directly**. Cross-module communication is exclusively via **MassTransit 8.x integration events** over RabbitMQ, using contracts from `Stoq.Shared.Contracts`.
- `Stoq.API` is a thin entry-point: controllers, SignalR hubs, DI composition. It references Application layer interfaces only.
- Domain layer: pure C#, zero framework dependencies.
- Application layer: MediatR handlers, ports (interfaces), DTOs, commands/queries. No EF Core, no HTTP clients.
- Infrastructure layer: EF Core DbContexts, HTTP clients, MassTransit producers/consumers, external adapters.

### C# & .NET 10 — Modern Language Features

Always prefer modern C# syntax. Use these features actively:

```csharp
// Primary constructors (C# 12)
public class CreateProductHandler(IProductRepository repository, IVisionService vision)
    : IRequestHandler<CreateProductCommand, ProductDto> { ... }

// Collection expressions (C# 12)
List<string> tags = ["food", "dairy"];
string[] units = ["g", "ml", "units"];
ReadOnlySpan<int> ids = [1, 2, 3];

// Required members (C# 11)
public class ProductDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}

// Raw string literals (C# 11) — use for SQL, JSON, prompts
var prompt = """
    Extract the list of grocery items from this receipt image.
    Return a JSON array with fields: name, quantity, unit.
    """;

// Pattern matching (C# 10+)
var message = item.Status switch
{
    ItemStatus.Low => "Low stock",
    ItemStatus.Expired => "Expired",
    _ => "OK"
};

// Records for immutable DTOs / value objects
public record ProductDto(Guid Id, string Name, string Brand, string Category);

// Null-coalescing assignment
_cache ??= new Dictionary<string, Product>();

// Using declarations (no braces)
using var stream = File.OpenRead(path);

// Global usings go in GlobalUsings.cs per project — don't repeat common namespaces in every file.

// Target-typed new
private readonly List<DomainEvent> _events = new();
```

### CQRS + MediatR

- Commands mutate state; Queries return data. Keep handlers small and focused.
- Validate commands with **FluentValidation** pipeline behaviour — never validate manually inside handlers.
- Queries should project directly to DTOs using EF Core `Select()` — never return entities.

```csharp
// Command
public record AddInventoryItemCommand(
    Guid HouseholdId,
    Guid ProductId,
    decimal Quantity,
    string Unit,
    DateOnly? ExpiryDate) : IRequest<InventoryItemDto>;

// Handler with primary constructor
public class AddInventoryItemHandler(
    IInventoryRepository repository,
    ICurrentUserService currentUser,
    IPublishEndpoint bus) : IRequestHandler<AddInventoryItemCommand, InventoryItemDto>
{
    public async Task<InventoryItemDto> Handle(AddInventoryItemCommand cmd, CancellationToken ct)
    {
        // ...
    }
}
```

### Controllers

- Always use `[ApiController]` — no Minimal API endpoints.
- Controllers are thin: validate auth, call MediatR, return result. No business logic.
- Return `IActionResult` or typed `ActionResult<T>`. Use `Ok()`, `Created()`, `NoContent()`, `NotFound()`, `BadRequest(ProblemDetails)`.
- Use `CancellationToken ct` on every action method — pass it downstream.
- **Request DTOs** live in `Stoq.ApiService/DTOs/` — one file per feature area (e.g. `HouseholdRequests.cs`, `ProductRequests.cs`). Never declare request records at the bottom of a controller file.

```csharp
[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddInventoryItemRequest request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var command = new AddInventoryItemCommand(request.ProductId, request.Quantity, userId.Value);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return BadRequest(Problem(result.Error));
    }
}
```

### C# Coding Style

- **Always use braces** on `if`, `else`, `foreach`, `while` — even for single-line bodies. Never write `if (x) return y;` on one line.
- **Never use ternary in `return` statements.** Convert `return condition ? A : B;` to an explicit `if`/`return` block:

```csharp
// Wrong
return result.IsSuccess ? Ok(result.Value) : BadRequest(Problem(result.Error));

// Correct
if (result.IsSuccess)
{
    return Ok(result.Value);
}

return BadRequest(Problem(result.Error));
```

- **`var` rules** (enforced by `.editorconfig`):
  - Use `var` when the type is stated on the right-hand side: `new T()`, `new T { }`, collection expressions, explicit casts.
  - Use `var` for built-in types (`int`, `string`, `bool`, `double`, `decimal`, etc.) — even for method returns.
  - Use `var` for `foreach` loop variables.
  - Use **explicit type** for all other method/property returns where the type is not obvious from the expression.

```csharp
// Correct — type apparent from RHS (new T) or built-in
var builder = WebApplication.CreateBuilder(args);
var now = DateTimeOffset.UtcNow;
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
var accessToken = tokenService.GenerateAccessToken(...); // string is a built-in
bool valid = await identityService.CheckPasswordAsync(...); // NO — bool is built-in, use var
var valid = await identityService.CheckPasswordAsync(...); // Correct

// Correct — explicit type for non-built-in method returns
UserInfo? user = await identityService.FindByEmailAsync(email, ct);
HouseholdMembership membership = await identityService.GetHouseholdMembershipAsync(userId, ct);
AppRole effectiveRole = membership.HouseholdRole ?? user.Role;
IdentityResult result = await userManager.CreateAsync(user, password);
List<AggregateRoot<Guid>> aggregates = context.ChangeTracker.Entries<AggregateRoot<Guid>>()...ToList();

// Wrong — var for non-built-in method return
var user = await identityService.FindByEmailAsync(email, ct);
var membership = await identityService.GetHouseholdMembershipAsync(userId, ct);
```

- **Always add a blank line before a single-line `//` comment**, unless the comment is the first line in a block or immediately follows another comment (SA1515).

```csharp
// Wrong
var x = DoSomething();
// This comment has no blank line above it
var y = DoMore();

// Correct
var x = DoSomething();

// This comment has a blank line above it
var y = DoMore();
```

- **`OnModelCreating` parameter must be named `modelBuilder`** to match the base class declaration (CA1725). Never use `builder` as the parameter name in `DbContext.OnModelCreating`.

- **Case-insensitive string comparison**: use `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` or `string.Equals(a, b, StringComparison.Ordinal)` — never `a.ToLower() == b.ToLower()` or `.Equals()` without `StringComparison`.

- **Case-insensitive database string search** (PostgreSQL): use `EF.Functions.ILike(column, pattern)` — never `column.ToLower().Contains(filter)` inside LINQ queries.

- **Static members before non-static members** (SA1204): always declare `static` fields, properties, and methods above instance ones within the same class.

- **Multi-line parameter lists** (SA1117): either all parameters on one line, or each parameter on its own line. Never mix.

- **Trailing commas in multi-line initializers** (SA1413): always add a trailing comma after the last element in multi-line object/collection initializers.

- **`static readonly` over constant arrays** (CA1861): use `static readonly` fields for arrays passed to methods that are called repeatedly and do not mutate the array.

- **Value type controller inputs** (S6964 / Sonar RSPEC-6964): value type properties on controller request DTOs must be `nullable`, `required`, or annotated with `[JsonRequired]` to prevent silent under-posting.

- **No `Impl` suffix on concrete classes**: name implementations after what they are, not what they implement. Prefer a noun that describes the mechanism (`SqlProductRepository`, `HttpExpoPushService`) or drop the suffix entirely when the context is unambiguous (`ProductRepository`, `ExpoPushService`). Never append `Impl` to any class name.

### Domain & Value Objects

- Inherit from `AggregateRoot` or `ValueObject` in `Stoq.Shared.Kernel`.
- Raise domain events inside aggregates; publish via MediatR `INotificationHandler` within the same module.
- Use private setters or `init` on aggregate properties; expose mutation only through methods.
- Value objects override `Equals` and `GetHashCode` (or use records).

```csharp
// Value object as record
public record Barcode(string Value)
{
    public static Barcode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < 8 or > 14) throw new DomainException("Invalid barcode length.");
        return new Barcode(value);
    }
}
```

### EF Core

- One `DbContext` per module, each targeting its own PostgreSQL schema (`identity`, `catalog`, `inventory`, `shopping`, `retailer`).
- Use `AsNoTracking()` for all read-only queries.
- Prefer `Select()` projections — never return entities from Application or API layers.
- Avoid N+1: use `Include` deliberately or project with joins.
- Set audit columns (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) via a `SaveChangesInterceptor` — never in handlers or controllers.
- Keep migrations small and descriptive. Name them meaningfully: `AddExpiryDateToInventoryItems`.

```csharp
// Good — projection query
var items = await context.InventoryItems
    .AsNoTracking()
    .Where(i => i.HouseholdId == householdId && i.ExpiryDate <= cutoff)
    .Select(i => new ExpiringItemDto(i.Id, i.ProductId, i.ExpiryDate!.Value, i.Quantity))
    .ToListAsync(ct);
```

### Async & Reliability

- `async/await` end-to-end. Never use `.Result`, `.Wait()`, or `Task.Run` to wrap sync code.
- Always accept and propagate `CancellationToken` — from controller actions down to EF Core and HTTP calls.
- Use **Polly** retry/circuit-breaker (via ServiceDefaults) for external HTTP calls (Open Food Facts, Claude API, SEFAZ).

### MassTransit 8.x

- Use version **8.x** (last fully open-source version before v9 licensing changes).
- All integration events must carry `InitiatedByUserId` (Guid).
- Use the **Outbox pattern** (`AddEntityFrameworkOutbox`) per module — events are committed in the same DB transaction as domain changes.
- Consumers live in Infrastructure; they never call another module's Application layer directly.

```csharp
// Integration event contract (in Stoq.Shared.Contracts)
public record StockLowIntegrationEvent(
    Guid HouseholdId,
    Guid ProductId,
    decimal CurrentQty,
    decimal MinQty,
    Guid InitiatedByUserId);
```

### Error Handling & Logging

- Use `ProblemDetails` for all error responses (configured globally via `AddProblemDetails()`).
- Define typed domain exceptions: `DomainException`, `NotFoundException`, `UnauthorizedException` — map them to HTTP status codes in a middleware or exception handler.
- Use `ILogger<T>` with structured logging. Log at appropriate levels. Include TraceId automatically via OpenTelemetry.
- Never catch `Exception` broadly unless in top-level middleware; catch specific exception types.

### OpenTelemetry

- All observability wired in `Stoq.ServiceDefaults.AddServiceDefaults()` — no per-project boilerplate.
- Add custom spans for key domain operations using `StoqActivitySource` from `Stoq.Shared.Kernel`.
- Emit custom metrics: `inventory_items_added_total`, `receipts_scanned_total`, `ai_vision_calls_total`.

### Security

- Short-lived JWT (15 min) + long-lived refresh token (30 days).
- JWT payload: `sub`, `email`, `systemRole`, `householdId`.
- `SystemRole` is orthogonal to household `Role` — check both where relevant.
- API Keys for `RetailerApiUser` are hashed (never stored in plain text). Validated via `Authorization: ApiKey {key}` header.
- Never expose raw coordinates through the Retailer API — only aggregated polygon checks.

### Build & Code Quality

- `Directory.Build.props` at `server/` root: `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true` (in CI), `LangVersion=latest`, `EnforceCodeStyleInBuild=true`.
- `Directory.Packages.props`: Central Package Management — no version attributes in individual `.csproj` files.
- `.editorconfig` at `server/` root: `indent_size=4`, `end_of_line=lf`, `charset=utf-8-bom`, PascalCase for types/methods, `_camelCase` for private fields.
- SonarAnalyzer, Roslynator, StyleCop run as Roslyn analysers at build time — fix all errors before committing.

### Testing

- Unit tests: test domain logic and MediatR handlers in isolation. Use `NSubstitute` or `Moq`.
- Integration tests: use `Testcontainers.PostgreSql` and `Testcontainers.RabbitMq` — no mocks for infrastructure.
- Use `FluentAssertions` for readable assertions.
- Coverage thresholds enforced in CI: 80% lines for backend.
- Tag tests: `[Trait("Category", "Unit")]` / `[Trait("Category", "Integration")]`.

---

## Mobile — React Native / Expo (mobile/)

### Architecture & Structure

Feature-based folder structure — all code for a domain area co-located:

```
mobile/src/
├── app/              # Navigation root (AuthStack + AppTabs)
├── features/
│   ├── auth/         # Login, Register, Social Auth screens + slice + RTK endpoints
│   ├── inventory/    # Inventory dashboard, product detail, add/consume screens
│   ├── catalog/      # Barcode scanner, photo recognition, manual product form
│   ├── shopping/     # Shopping list, check-off, manual add
│   ├── receipts/     # Receipt camera, NF-e QR scan, parsed items confirmation
│   ├── household/    # Household creation, invite code, member/address management
│   └── location/     # Address form, GPS permission, nearby-store suggestions
├── store/            # Redux store + root reducer + Redux Persist config
├── api/              # RTK Query baseApi + JWT interceptor + injected slices
├── hooks/            # useSignalR, useBarcode, useCamera, usePushNotifications, useLocation
├── components/       # Shared Paper-based components (AppButton, AppCard, ProductCard…)
├── navigation/       # Stack + tab definitions
├── theme/            # MD3 theme tokens, dark/light variants, spacing
├── i18n/             # i18next setup, en/ and pt-BR/ locale files
└── utils/            # Date helpers, barcode validators, quantity formatters, offline queue
```

### React Native Patterns

- Functional components + hooks only. No class components.
- Keep props and state **strongly typed**. Avoid `any`.
- Presentational components in `src/components/`; business logic in hooks or RTK Query.
- Use early returns and guard clauses to reduce nesting.
- Memoize (`useMemo`, `useCallback`, `React.memo`) only when a measurable perf issue exists.

### UI — React Native Paper (Material Design 3)

- Use **React Native Paper 5.x** components exclusively for all UI. No raw `View`/`Text` styling for interactive elements.
- All text inputs use `<TextInput mode="outlined" />` from Paper. Never use bare React Native `TextInput` in screens.
- All colours via `useTheme()` hook — **never hardcode colours** in component files.
- Brand primary: `#1E3A5F`. Secondary/accent: `#2E7D32`. Defined in `src/theme/index.ts` via `MD3LightTheme.colors` spread.
- Dark mode toggled via `uiSlice` (Redux). `PaperProvider` swaps themes. Preference persisted in AsyncStorage.

### State Management

| Layer | What lives here |
|---|---|
| RTK Query | Server state: inventory, shopping list, catalog, household. Auto-refetch on focus/reconnect. Cache tags invalidated on mutations. |
| `authSlice` | `accessToken`, `refreshToken`, `userId`, `householdId`, `systemRole`. Persisted to Expo SecureStore. |
| `locationSlice` | GPS coords (not persisted), permission status, saved address. |
| `inventorySlice` | Optimistic offline queue for consume/add. Flushed on reconnect. |
| `shoppingSlice` | Local check-off state during active shopping trip. |
| `uiSlice` | Filters, scan mode, modal state. Not persisted. |

- Server data lives in **RTK Query cache only** — no manual `useEffect` fetching.
- Use **Redux Persist** for `inventorySlice` and `shoppingSlice` (AsyncStorage) to enable offline access.
- Tokens stored in **Expo SecureStore** — never AsyncStorage for sensitive data.

### Data Fetching

- Always implement **loading / empty / error states** for every screen that fetches data.
- RTK Query `baseQuery` intercepts 401 → calls `POST /api/auth/refresh` → retries original request.
- RTK Query automatically sends `Accept-Language: {{locale}}` header on every request.

**Error state rules — never collapse these into the same message:**

| State | Condition | Message key | UX |
|---|---|---|---|
| Server/auth error | `isError === true` | `common.loadError` | Inline error text in error colour + Retry button |
| Empty results | `!isError && data?.length === 0` | domain-specific empty key | Neutral grey text, no retry |
| Network error | fetch throws / no response | `common.networkError` | `<OfflineBanner />` or inline |

```tsx
// Correct pattern
{isError ? (
  <ErrorState message={t('common.loadError')} onRetry={refetch} />
) : data?.length === 0 ? (
  <EmptyState message={t('catalog.list.empty')} />
) : (
  <FlatList data={data} ... />
)}
```

### Real-Time (SignalR)

- `useSignalR` hook establishes connection on app foreground and joins the household group.
- On receiving server-pushed events, invalidate the relevant RTK Query cache tags — do not manually update state.
- During an active shopping trip, defer `ShoppingListUpdated` events to avoid UI flicker.

### Offline Strategy

| Action | Behaviour |
|---|---|
| View inventory / shopping list | Works fully offline via Redux Persist cache. |
| Check off item | Optimistic update in `shoppingSlice`; queued mutation synced on reconnect. |
| Consume inventory item | Optimistic update in `inventorySlice` offline queue; synced on reconnect. |
| Barcode scan / Receipt scan / Add product | Requires connectivity. Shows `<OfflineBanner />` and disables action. |

### Internationalisation (i18n)

- Languages: **English (`en`)** and **Brazilian Portuguese (`pt-BR`)**.
- Library: `i18next` + `react-i18next` + `expo-localization`.
- **Every user-visible string goes through `t()`** — no hardcoded strings in JSX. Enforced by `eslint-plugin-i18next`.
- Use `useTranslation('featureName')` with namespaces — one JSON file per feature per language.
- Pluralisation: use i18next `_one` / `_other` suffixes.
- Dates and numbers: `Intl.DateTimeFormat` and `Intl.NumberFormat` with active locale — never hardcode formats.
- CI check: script diffs `en/` and `pt-BR/` key sets; build fails on missing key in either language.
- Language detection order: (1) AsyncStorage user preference → (2) device locale → (3) fallback `en`.

### TypeScript

- Strict mode enabled. No `any`. No `@ts-ignore` without a comment explaining why.
- Use `unknown` instead of `any` for catch blocks and untyped external data.
- RTK Query API types auto-generated from `openapi.json` via `openapi-typescript` — use generated types; don't hand-roll them.
- i18next type-safety via declaration merging in `src/i18n/types.ts` — missing/mistyped keys caught at compile time.

### Navigation

| Navigator | Screens |
|---|---|
| `AuthStack` | Splash → Login → Register → ForgotPassword → SocialAuthCallback |
| `AppTabs` | Inventory · Shopping List · Scan (FAB) · Profile |
| `InventoryStack` | InventoryDashboard → ProductDetail → AddProduct → ConsumeProduct |
| `ShoppingStack` | ShoppingList → AddShoppingItem → ReceiptScan → ReceiptConfirm |
| `ProfileStack` | Profile → HouseholdSettings → ManageMembers → AddressManager → RetailerConsents → Notifications → AppSettings |
| Modals | ExpiryPicker, QuantityEditor, InviteCode, ScanResult |

### Accessibility

- Meaningful `accessibilityLabel` and `accessibilityHint` on all interactive elements.
- Minimum 44×44pt tap targets (Paper components handle this by default).
- Keyboard navigation and sensible focus handling.
- Use logical spacing props (`marginStart`/`marginEnd`) — RTL-ready for future languages.
- Color contrast WCAG AA minimum. Never rely on color alone to convey state.

### Testing

- Test user-visible behaviour, not implementation details.
- Cover happy path + validation + error states for critical flows.
- Use `jest-expo` preset. Coverage threshold: 70% lines enforced in CI.
- `npx tsc --noEmit` and `npx eslint src/ --max-warnings 0` must pass before committing.

---

## Key Domain Concepts (quick reference)

| Concept | Rule |
|---|---|
| `HouseholdId` | All data is scoped to a household. Always filter by `HouseholdId` — never return cross-household data. |
| `InitiatedByUserId` | Every integration event carries this field. Every audit log entry carries this field. |
| Audit columns | `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` on every table. Set automatically by EF Core interceptor — never set manually. |
| `SystemRole` | `Consumer`, `RetailerAdmin`, `RetailerViewer`, `RetailerApiUser`, `PlatformAdmin`. Orthogonal to household `Role` (Admin/Member/Viewer). |
| Receipt scanning | Highest-impact feature. NF-e QR Code is primary (Brazil); photo fallback via Claude Vision API. Always return parsed items for user confirmation before committing to inventory. |
| AI Vision calls | Claude Vision API (`claude-sonnet`). Cache parsed results. Graceful error handling with user-friendly fallback. |
| Barcode lookup | Open Food Facts first; fallback to manual entry. Consider Cosmos DB (Brazil) as secondary source. |
| Module isolation | A module's projects must **never** reference another module's projects. Anti-corruption adapters (read-only) are the only exception. |
| MassTransit version | **8.x only** (e.g. 8.3.x). Do not upgrade to v9 — commercial licensing. |
| Tokens | JWT access token: 15 min. Refresh token: 30 days, stored in Expo SecureStore. |
| Privacy | No background GPS tracking. `LocationSnapshots` purged after 24h. Retailers never receive raw coordinates. |

---

## CI/CD Constraints

- Each workspace has an independent pipeline triggered by **path filters** — a mobile change must not trigger the .NET build.
- `server.yml`: restore → build (TreatWarningsAsErrors) → static analysis → unit tests (80% coverage) → integration tests → generate OpenAPI spec → Docker → deploy.
- `mobile.yml`: `npm ci` → `tsc --noEmit` → ESLint (0 warnings) → Jest (70% coverage) → Expo export check → EAS build (main only).
- `api-codegen.yml`: triggered by changes to `api-contracts/openapi.json` → regenerates TypeScript types → opens automated PR.
- The OpenAPI spec is the contract between server and mobile. Breaking changes to the spec must be intentional and coordinated.

---

## Out of Scope (v1.0 — do not implement)

- Fixed camera / smart shelf monitoring.
- Price comparison between retailers.
- Automated delivery or cart integration.
- Nutritional tracking or dietary planning.
- Web dashboard (mobile-only for v1.0).
- Multi-household management from a single account.
- MassTransit v9+ features.
