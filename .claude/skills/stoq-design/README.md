# Stoq — Design System

Stoq is a **mobile-first smart inventory and grocery manager for households**. It lets a family track what they have at home, scan grocery receipts (via NF-e QR code or a photo of the cupom fiscal) to bulk-add stock, watch expiry dates, and manage a shared shopping list across household members.

- **Platform:** React Native (Expo managed workflow), Android + iOS
- **UI framework:** [React Native Paper](https://callstack.github.io/react-native-paper/) — **Material Design 3** (MD3) light theme
- **Primary locale:** Brazilian Portuguese (pt-BR); English fallback present
- **Architecture:** Redux Toolkit + RTK Query, SignalR for household real-time sync, redux-persist, Expo Camera for scanning

This design system documents the brand's visual language and ships a faithful, reusable **UI kit** so designers and agents can produce on-brand Stoq screens and assets.

---

## Sources

This system was reverse-engineered from the read-only mobile codebase mounted at `mobile/`:

| What | Where |
|---|---|
| Theme tokens | `mobile/src/theme/index.ts` |
| All screens | `mobile/src/screens/{auth,catalog,inventory,shopping,household,profile}/` |
| Navigation (5-tab + center scan FAB) | `mobile/src/navigation/MainTabNavigator.tsx` |
| Copy / tone (pt-BR + en) | `mobile/src/i18n/locales/{pt-BR,en}.json` |
| Brand assets | `mobile/assets/` (icon, splash, adaptive-icon — **all currently Expo placeholders**) |

> No Figma file or slide deck was provided. No production logo exists yet — the app icon is the default Expo bullseye placeholder. See **Caveats** at the bottom.

---

## The current state (important)

The theme overrides **only six** MD3 tokens (`primary`, `onPrimary`, `primaryContainer`, `secondary`, `surface`, `error`). **Every other token stays at the MD3 baseline default**, which is purple-tinted (`secondaryContainer #E8DEF8`, `surfaceVariant #E7E0EC`, `onSurfaceVariant #49454F`, `tertiary #7D5260`, `onPrimaryContainer #21005D`). That is why:

- The **"Adicionado auto." chip** on the shopping list renders in pale purple.
- The **FAB icon** (`onPrimaryContainer`) is dark purple on the light-green FAB.
- **Secondary text and dividers** carry a faint mauve cast instead of reading as clean greens/greys.

This is the root cause of the brief's "mostly white/grey with minimal accent" feeling. This design system keeps the real green brand tokens and **replaces the stray purple neutrals with a green-tinted neutral scale** (documented in `colors_and_type.css`), so designs stay cohesive. Where a token is a deliberate improvement over what's shipping, it is flagged inline.

---

## Index — what's in this folder

| File / folder | Purpose |
|---|---|
| `README.md` | This file — product context, content + visual foundations, iconography |
| `colors_and_type.css` | All design tokens as CSS vars: color palette, neutrals, semantic colors, type scale, spacing, radii, elevation |
| `SKILL.md` | Agent-Skill manifest so this system works inside Claude Code |
| `assets/` | Brand assets — Stoq logo (icon, mark, lockups), source Expo icons |
| `preview/` | Small HTML specimen cards that populate the Design System tab |
| `ui_kits/mobile/` | High-fidelity, interactive recreation of the Stoq app (screens + reusable JSX components) |

---

## CONTENT FUNDAMENTALS

**Language & locale.** Primary copy is **Brazilian Portuguese (pt-BR)**, written for everyday households. English exists as a fallback but pt-BR is the design target. Always write or review copy in pt-BR first.

**Voice — warm, plain, second-person.** The product addresses the user directly with the implicit *você* and possessives: *"Configure **sua** residência"*, *"**Seu** estoque está vazio"*, *"Verifique **seu** e-mail"*. It is helpful and reassuring, never clever or jokey.

**Tone is instructional and low-anxiety.** Empty states pair a calm statement with a concrete next step: *"Seu estoque está vazio"* → *"Comece adicionando os produtos que você tem em casa."* Errors are gentle and recoverable: *"Algo deu errado. Tente novamente."*, *"Sem conexão. Verifique sua internet e tente novamente."*

**Casing.** **Sentence case everywhere** — titles, buttons, labels. (*"Adicionar primeiro item"*, *"Criar residência"*, not Title Case.) Screen headers occasionally use a Capitalized first word only (*"Alertas de Validade"*, *"Escanear Cupom"*). Buttons are imperative verbs: *Entrar, Criar conta, Adicionar ao estoque, Confirmar itens, Consumir*.

**Domain vocabulary (keep these exact terms):**
- **residência** — the shared household
- **estoque** — inventory / home stock
- **cupom fiscal** / **NF-e** — Brazilian receipt + its tax QR code
- **lote** — a batch (same product, same expiry)
- **estoque baixo** — low stock; **vencimento / validade** — expiry
- **código de convite** — household invite code

**Numbers & formatting.** Quantities are `{value} {unit}` (*"2 Litros"*, *"500 Gramas"*). Units come from a fixed enum (Unidades, Gramas, Quilogramas, Mililitros, Litros, Peças, Caixas, Pacotes, Garrafas, Latas, Sacolas, Dúzias). Dates respect household country: **DD/MM/AAAA** for Brazil, MM/DD/YYYY for US; the expiry input format is **AAAA-MM-DD**.

**Punctuation.** Uses the ellipsis character `…` (not three dots) in loading/placeholder text: *"Carregando…"*, *"Buscar produtos…"*, *"Lendo cupom…"*.

**Pluralization** is handled with i18next count forms: *"{{count}} dia restante"* / *"{{count}} dias restantes"*, *"{{count}} item próximo ao vencimento"* / *"{{count}} itens..."*.

**No emoji.** The product uses **zero emoji** in UI copy. Iconography is carried entirely by Material icons. Do not introduce emoji.

---

## VISUAL FOUNDATIONS

**Overall feel.** Clean, utilitarian Material Design 3. Generous white space, list-driven layouts, a single confident green accent. It should feel like a tidy, trustworthy kitchen helper — not playful, not luxurious.

**Color.** A **green** brand built on `primary #2D6A4F` (deep forest green) with a `secondary #52B788` (medium mint) and a soft `primaryContainer #B7E4C7` for banners and chips. Surfaces are near-white (`surface #F8F9FA` on a pure `#FFFFFF` background). Error is a strong `#BA1A1A`. Accent is used **sparingly and purposefully** — active tab, primary buttons, FABs, status text — over a large neutral field. See the "current state" note above re: stray purple defaults.

**Typography.** **Roboto** throughout (the MD3 default; no custom font is loaded by the app). The full MD3 type scale is in use — `headlineMedium` for auth titles, `titleLarge/Medium/Small` for section and card headers, `bodyLarge/Medium/Small` for content, and `labelLarge/Small` for buttons, chips and status. Titles and labels use Roboto **Medium (500)**; body is **Regular (400)**. Section eyebrows (e.g. "Lotes") are `titleSmall` **uppercased with +0.5 letter-spacing**.

**Spacing.** A strict **4-pt grid**: 4 / 8 / 12 / 16 / 24 / 32. Screen edge padding is **16**. Vertical rhythm inside cards/rows is 8–12. Centered empty states use 12 gap and 32 horizontal padding.

**Corner radii.** MD3 radii: **list icon chips are full circles** (40×40, r=20), the **center scan button** is a 56×56 circle (r=28), **cards** use ~12, **FABs** 16, **chips** are pill/full. Inputs are MD3 `outlined` (≈4 radius).

**Cards.** Two patterns: (1) **flat list rows** — no border, separated by hairline `Divider`s, with a leading circular icon chip; (2) **Surface/Card** — subtle MD3 elevation (no border), used for the inventory summary and receipt items. Cards never use colored left-border accents.

**Backgrounds.** Always flat solid color — pure white content over off-white surfaces. **No gradients, no photographic backgrounds, no textures or patterns.** Product imagery (when present) appears only as small thumbnails inside rows/cards.

**Elevation & shadow.** MD3 elevation. Bottom action bars and the scan button use `elevation: 4` (shadow: y2, blur4, ~25% black). FABs float at the standard MD3 elevation. No heavy or colored shadows.

**Banners & chips.** Full-width tappable **banners** (e.g. the expiry banner) flip between `errorContainer` (alert) and a neutral surface (calm). **Chips** are compact pills: low-stock uses `errorContainer`, auto-added uses a container tint, expiry-urgency chips are `outlined` with the urgency color as border + text.

**Status / urgency color logic.** Expiry urgency: **≤1 day → error red**, **≤3 days → warning amber** (currently MD3 mauve `tertiary` — replaced with amber here, flagged), **>3 days → neutral outline**. Receipt-scan status: Pending/Processing → primary, Parsed → secondary, Confirmed → muted neutral, Failed → error.

**Buttons.** MD3 variants, all used: `contained` (primary green, white text — main actions), `outlined` (green border + text — secondary), `text` (green — tertiary/links), `contained-tonal` (light-green container — e.g. "Verificar produtos a vencer"). Full-width contained buttons anchor bottom action bars.

**Motion.** Standard Material motion — no custom animation. Transitions are native stack pushes and tab fades. `ActivityIndicator` (circular spinner) for all loading. **Press states** are MD3 ripple (`TouchableRipple`, ripple tinted with the surface-variant). **Hover** is not a concern (touch-first). No bounce, no parallax, no decorative looping animation.

**Layout rules.** Fixed bottom **5-tab bar** (Estoque · Lista · [Scan] · Catálogo · Perfil) with a raised center circular scan button that overhangs the bar by 14px. Floating **FAB**(s) bottom-right (`FAB.Group` for multi-action). Bottom action bars are fixed, full-width, on `surface`, with bottom safe-area padding (≈32). Lists scroll under a sticky search bar / banner.

> **Known bug to design around:** bottom tab labels are being clipped on small screens. The kit uses a shorter label set and tighter type to prevent truncation.

**Transparency / blur.** Essentially none — the UI is fully opaque. Disabled/confirmed items use **opacity 0.5–0.6** (e.g. checked shopping items get strikethrough + 0.5; confirmed receipt scans 0.6).

---

## ICONOGRAPHY

**System:** **Material Community Icons**, shipped via `@expo/vector-icons` (`MaterialCommunityIcons`). This is a single, consistent, **outline-forward** icon family — line icons for inactive/secondary states and **filled variants for active tab states** (e.g. `fridge-outline` → `fridge`, `cart-outline` → `cart`, `account-circle-outline` → `account-circle`). Default sizes: 20–26px (tab icons 24, scan icon 26, inline list icons 18–22).

**Key icons in use:**
- Tabs: `fridge` (Estoque), `cart` (Lista), `barcode-scan` (Scan, center), `package-variant` (Catálogo), `account-circle` (Perfil)
- Actions: `plus`, `close`, `check`, `check-all`, `pencil-outline`, `trash-can-outline`, `minus-circle-outline`, `plus-circle-outline`, `cart-plus`, `refresh`, `filter-variant`
- Scanning: `barcode-scan`, `qrcode-scan`, `receipt-text-outline`
- Status / nav: `calendar-alert`, `chevron-right`, `home-account`, `home-outline`, `logout`, `eye`/`eye-off`, `google`

**Emoji:** never used. **Unicode glyphs:** only the ellipsis `…`. No icon fonts beyond Material Community Icons.

**In this kit:** the UI kit and specimen cards load **Material Community Icons via CDN** (`@mdi/font`) so the exact same glyph set renders in HTML — no substitution. Icons are referenced by their `mdi-*` class names, which map 1:1 to the React Native icon names.

> **Logo.** The app shipped with only the default Expo placeholder icon, so a dedicated **Stoq logo** was designed for this system. The mark is an **"S" monogram with a sprout leaf** — combining *stock* (the S) with *fresh groceries* (the leaf) — set in the brand green. The logotype "Stoq" is set in **Manrope ExtraBold** (a friendly geometric sans, distinct from the Roboto UI font). Files in `assets/`:
>
> | File | Use |
> |---|---|
> | `stoq-icon.svg` | App icon — rounded green square, white monogram, mint sprout. Replaces the Expo placeholder. |
> | `stoq-mark.svg` | The monogram alone (green), for light backgrounds / favicons |
> | `stoq-logo.svg` | Horizontal lockup (icon + wordmark) for light backgrounds |
> | `stoq-logo-white.svg` | Reversed lockup for green / dark backgrounds |
>
> The monogram stays legible down to ~22px. `source-*.png` are the original Expo placeholders, kept for reference only.
