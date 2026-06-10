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
