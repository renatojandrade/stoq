import React from 'react';
import { Image, StyleSheet, View } from 'react-native';
import { Icon, useTheme } from 'react-native-paper';

// ── Category tints ────────────────────────────────────────────────────────────
// Source: stoq-design/ui_kits/mobile/theme.js — green-family + neutral accents.
// Kept as constants so they are stable independently of the MD3 theme object.
const TINT = {
  dairy:   '#E3F0FF',
  produce: '#E7F6E1',
  pantry:  '#FBEFD8',
  meat:    '#FBE4E4',
  bev:     '#E6EEFB',
  frozen:  '#E2F4F4',
} as const;

type TintKey = keyof typeof TINT;
const TINT_KEYS = Object.keys(TINT) as TintKey[];

// ── Keyword → category mapping ────────────────────────────────────────────────
// Matches common Brazilian grocery names; falls back to a deterministic tint
// derived from the first letter when no keyword matches.
interface CategoryInfo { icon: string; tint: string; }

const RULES: Array<{ pattern: RegExp; icon: string; tint: string }> = [
  {
    pattern: /leite|iogurte|queijo|manteiga|requeijão|creme de leite|nata|ovo|egg|milk|yogurt|butter|cheese/,
    icon: 'bottle-soda-outline', tint: TINT.dairy,
  },
  {
    pattern: /frango|carne|peixe|atum|sardinha|linguiça|bacon|chicken|meat|beef|fish|hambúrguer|presunto|salsicha|peito/,
    icon: 'food-drumstick-outline', tint: TINT.meat,
  },
  {
    pattern: /arroz|feijão|macarrão|massa|café|açúcar|farinha|aveia|sal|azeite|óleo|pão|biscoito|bolacha|cereal|granola|rice|flour|coffee|sugar|bread|pasta/,
    icon: 'rice', tint: TINT.pantry,
  },
  {
    pattern: /tomate|banana|maçã|pera|laranja|limão|alface|couve|cenoura|batata|cebola|alho|verdura|fruta|legume|apple|tomato|fruit|vegetable/,
    icon: 'food-apple-outline', tint: TINT.produce,
  },
  {
    pattern: /sorvete|congelado|frozen|ice/,
    icon: 'snowflake', tint: TINT.frozen,
  },
  {
    pattern: /refrigerante|suco|cerveja|água|vinho|detergente|sabão|amaciante|shampoo|condicionador|limpeza|higiene|beverage|juice|water|wine/,
    icon: 'bottle-soda', tint: TINT.bev,
  },
];

function classify(name: string): CategoryInfo {
  const n = name.toLowerCase();
  for (const rule of RULES) {
    if (rule.pattern.test(n)) {
      return { icon: rule.icon, tint: rule.tint };
    }
  }

  // Deterministic fallback: cycle through tint keys by first character.
  const idx = ((name.charCodeAt(0) | 0x20) - 97 + TINT_KEYS.length) % TINT_KEYS.length;
  return { icon: 'package-variant-closed', tint: TINT[TINT_KEYS[idx]] };
}

// ── Component ─────────────────────────────────────────────────────────────────
// Matches Thumb from ui_kits/mobile/components.jsx:
//   size=46, borderRadius=14, icon at size/2, icon opacity 0.78, flex:none.

interface Props {
  name: string;
  imageUrl?: string | null;
  size?: number;
}

export default function ProductThumb({ name, imageUrl, size = 46 }: Props) {
  const { colors } = useTheme();
  const iconSize = Math.round(size / 2);

  if (imageUrl) {
    return (
      <Image
        source={{ uri: imageUrl }}
        style={{ width: size, height: size, borderRadius: 14 }}
        resizeMode="cover"
        accessibilityIgnoresInvertColors
      />
    );
  }

  const { icon, tint } = classify(name);

  return (
    <View style={[styles.thumb, { width: size, height: size, backgroundColor: tint }]}>
      <View style={styles.iconLayer}>
        <Icon source={icon} size={iconSize} color={colors.onSurface} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  thumb: {
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
    flexShrink: 0,
    overflow: 'hidden',
  },
  iconLayer: {
    opacity: 0.78,
  },
});
