/**
 * APPENDIX — target src/theme/index.ts for the Stoq app.
 *
 * This is the corrected React Native Paper (MD3) theme that bakes in the
 * full green token set. Today the app overrides only 6 colors, so the rest
 * fall back to MD3's purple-tinted defaults. This overrides every role that
 * the UI actually touches, using the values from `colors_and_type.css`.
 *
 * Drop this in as the new `src/theme/index.ts`. No component logic changes.
 * (ExpiryAlertsScreen already reads `colors.tertiary` for the "≤3 days"
 *  warning — setting tertiary to amber here fixes that screen automatically.)
 */
import { MD3LightTheme } from 'react-native-paper';

export const theme = {
  ...MD3LightTheme,
  colors: {
    ...MD3LightTheme.colors,

    // ---- Brand green ----
    primary: '#2D6A4F',
    onPrimary: '#FFFFFF',
    primaryContainer: '#B7E4C7',
    onPrimaryContainer: '#0D3321',     // was MD3 purple #21005D (e.g. FAB icon)

    secondary: '#52B788',
    onSecondary: '#FFFFFF',
    secondaryContainer: '#D6F0E1',     // was MD3 purple #E8DEF8 ("Adicionado auto." chip)
    onSecondaryContainer: '#0F3D29',

    // ---- Tertiary repurposed as WARNING (amber) ----
    // Used by ExpiryAlertsScreen for items expiring in ≤3 days.
    tertiary: '#B26A00',               // was MD3 mauve #7D5260
    onTertiary: '#FFFFFF',
    tertiaryContainer: '#FFDFB0',
    onTertiaryContainer: '#3A2200',

    // ---- Neutrals (green-tinted, replacing purple defaults) ----
    background: '#FFFFFF',
    onBackground: '#1A1C1B',
    surface: '#F8F9FA',
    onSurface: '#1A1C1B',
    surfaceVariant: '#DDE5DF',          // was MD3 #E7E0EC — icon-chip backgrounds
    onSurfaceVariant: '#404944',        // was MD3 #49454F — secondary text
    outline: '#707972',                 // was MD3 #79747E — borders/icons
    outlineVariant: '#C3C9C4',          // was MD3 #CAC4D0 — hairline dividers

    // ---- Error (unchanged brand value) ----
    error: '#BA1A1A',
    onError: '#FFFFFF',
    errorContainer: '#FFDAD6',
    onErrorContainer: '#410002',
  },
};
