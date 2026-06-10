# Stoq Mobile — UI Kit

A high-fidelity, interactive recreation of the **Stoq** React Native (Expo / React Native Paper · MD3) app, rebuilt in React + HTML so designers and agents can assemble on-brand Stoq screens quickly.

It is a **visual recreation**, not production code — interactions are faked, data is mock. Component implementations are cosmetic-only; they mirror the look of the real React Native Paper screens, not their logic.

## Run it
Open `index.html`. It boots on the **Login** screen → tap **Entrar** to enter the app (a **skeleton loading state** shows briefly). From there:
- **Bottom tabs:** Estoque · Lista · [Scan] · Catálogo · Perfil
- **Estoque** → tap the red expiry banner (→ Alertas de Validade) or any product row (→ detail + lotes). The **+** FAB → Adicionar ao estoque form
- **Lista** → check items off, open the **+** speed-dial → "Escanear cupom"
- **Scan button** (center) → **camera** → shutter → **Confirmar itens** (redesigned)
- **Catálogo** → filter chips (Todos / Gerais / Minha casa)

## Files
| File | Contents |
|---|---|
| `theme.js` | Design tokens (`window.STOQ.c`) + mock data (inventory, shopping, catalog, scans, receipt items) |
| `components.jsx` | Primitives: `Icon`, `Btn`, `Chip`, `Thumb`, `SearchBar`, `Banner`, `Fab`, `EmptyState`, `Divider`, `Snackbar`, `AppHeader`, `StatusBar`, `TabBar`, `IconButton` |
| `screens.jsx` | `LoginScreen`, `InventoryScreen`, `ExpiryScreen`, `DetailScreen`, `ShoppingScreen`, `CatalogScreen`, `ProfileScreen`, `ScanListScreen` |
| `screens-extra.jsx` | `LoadingScreen` (skeleton), `ScanCameraScreen`, `AddItemFormScreen` |
| `ReceiptConfirm.jsx` | `ReceiptConfirmScreen` — the redesigned, less-dense confirmation screen |
| `index.html` | Phone frame + simple navigation router that wires it all together |

## What was intentionally improved (vs. shipping app)
These are the brief's design goals, applied — flagged so they're easy to spot:
1. **Richer product cards** — grey circle icons replaced with **tinted category thumbnails** (`Thumb`). When real product images exist, drop them into the same 46px rounded square.
2. **Better color usage** — stray MD3 purple defaults replaced with green-aligned tokens (auto-added chip, FAB icon, expiry-warning amber). See `../../colors_and_type.css` and the "Purple Defaults — Flagged" card.
3. **ReceiptConfirm redesign** — dense stacked outlined inputs → grouped cards with a colored summary header, match-status chips, and an optional/de-emphasized expiry affordance.
4. **Tab labels** — shortened (Estoque / Lista / Catálogo / Perfil) and constrained so they don't clip on small screens.
5. **Empty / loading states** — consistent `EmptyState` pattern (icon + title + subtitle + action).

## Fidelity notes
- Type: Roboto (MD3 default). Icons: Material Community Icons via `@mdi/font` CDN — `mdi-*` class names map 1:1 to the React Native icon names.
- Behavior preserved: navigation structure, 5-tab + center-scan layout, FAB.Group speed dial, checkable shopping items, receipt-scan status states.
- The `google` brand glyph was dropped from MDI in v6+, so the "Continuar com Google" button shows no leading icon — cosmetic only.
