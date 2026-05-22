import React, { useCallback } from 'react';
import { FlatList, RefreshControl, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Chip,
  Divider,
  List,
  Snackbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useGetExpiringItemsQuery, useTriggerExpiryScanMutation } from '../../store/api/inventoryApi';
import type { InventoryItemDto } from '../../store/api/inventoryApi';

const DAYS_AHEAD = 7;

function daysUntil(expiryDateStr: string): number {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const expiry = new Date(expiryDateStr);
  expiry.setHours(0, 0, 0, 0);
  return Math.round((expiry.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
}

function urgencyColor(days: number, colors: { error: string; tertiary: string; outline: string }): string {
  if (days <= 1) {
    return colors.error;
  }
  if (days <= 3) {
    return colors.tertiary;
  }
  return colors.outline;
}

export default function ExpiryAlertsScreen() {
  const { colors } = useTheme();
  const { t } = useTranslation();

  const {
    data: items,
    isLoading,
    isFetching,
    isError,
    refetch,
  } = useGetExpiringItemsQuery(DAYS_AHEAD);

  const [triggerScan, { isLoading: isScanning, isSuccess: scanDone }] =
    useTriggerExpiryScanMutation();

  const handleScan = useCallback(async () => {
    await triggerScan();
    refetch();
  }, [triggerScan, refetch]);

  const renderItem = useCallback(
    ({ item }: { item: InventoryItemDto }) => {
      const days = daysUntil(item.expiryDate!);
      const chipColor = urgencyColor(days, colors);

      const expiryLabel =
        days < 0
          ? t('inventory.expiry.expired')
          : days === 0
            ? t('inventory.expiry.today')
            : t('inventory.expiry.daysLeft', { count: days });

      return (
        <List.Item
          title={item.productName}
          description={`${item.quantity} ${item.unit}${item.productBrand ? ` · ${item.productBrand}` : ''}`}
          right={() => (
            <Chip
              compact
              style={[styles.chip, { borderColor: chipColor }]}
              textStyle={{ color: chipColor }}
              mode="outlined"
            >
              {expiryLabel}
            </Chip>
          )}
        />
      );
    },
    [colors, t],
  );

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator animating size="large" />
      </View>
    );
  }

  if (isError) {
    return (
      <View style={styles.center}>
        <Text variant="bodyMedium" style={{ color: colors.error, marginBottom: 12 }}>
          {t('common.loadError')}
        </Text>
        <Button mode="outlined" onPress={refetch}>
          {t('common.retry')}
        </Button>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.scanRow}>
        <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant, flex: 1 }}>
          {t('inventory.expiry.scanHint')}
        </Text>
        <Button
          mode="contained-tonal"
          onPress={handleScan}
          loading={isScanning}
          disabled={isScanning}
          compact
        >
          {t('inventory.expiry.runScan')}
        </Button>
      </View>

      <Divider />

      <FlatList
        data={items ?? []}
        keyExtractor={(item) => item.id}
        renderItem={renderItem}
        refreshControl={<RefreshControl refreshing={isFetching} onRefresh={refetch} />}
        ItemSeparatorComponent={Divider}
        ListEmptyComponent={
          <View style={styles.center}>
            <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant }}>
              {t('inventory.expiry.empty')}
            </Text>
          </View>
        }
        contentContainerStyle={items?.length === 0 ? styles.emptyContainer : undefined}
      />

      <Snackbar visible={scanDone} onDismiss={() => {}} duration={3000}>
        {t('inventory.expiry.scanComplete')}
      </Snackbar>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 24,
  },
  emptyContainer: {
    flex: 1,
  },
  scanRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
    gap: 12,
  },
  chip: {
    alignSelf: 'center',
  },
});
