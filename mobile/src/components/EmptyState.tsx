import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Icon, Text, useTheme } from 'react-native-paper';

interface Props {
  icon: string;
  title: string;
  subtitle?: string;
  action?: React.ReactNode;
}

export default function EmptyState({ icon, title, subtitle, action }: Props) {
  const { colors } = useTheme();
  return (
    <View style={styles.container}>
      <View style={styles.iconWrap}>
        <Icon source={icon} size={52} color={colors.onSurfaceVariant} />
      </View>
      <Text variant="titleMedium" style={{ color: colors.onSurface, textAlign: 'center' }}>
        {title}
      </Text>
      {subtitle ? (
        <Text variant="bodyMedium" style={[styles.subtitle, { color: colors.onSurfaceVariant }]}>
          {subtitle}
        </Text>
      ) : null}
      {action ? <View style={styles.actions}>{action}</View> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 12,
    paddingHorizontal: 40,
  },
  iconWrap: { opacity: 0.5 },
  subtitle: { textAlign: 'center', maxWidth: 280 },
  actions: { marginTop: 4, gap: 10, width: '100%', alignItems: 'center' },
});
