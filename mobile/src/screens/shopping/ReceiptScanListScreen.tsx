import React from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Divider,
  Icon,
  Text,
  TouchableRipple,
  useTheme,
} from 'react-native-paper';
import SkeletonList from '../../components/SkeletonList';
import EmptyState from '../../components/EmptyState';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { ShoppingStackParamList } from '../../navigation/ShoppingNavigator';
import {
  useGetCorrectionsQuery,
  type ParseCorrectionStatus,
  type ParseCorrectionSummaryDto,
} from '../../store/api/receiptsApi';

type Props = NativeStackScreenProps<ShoppingStackParamList, 'ReceiptScanList'>;

function statusLabel(status: ParseCorrectionStatus, t: (k: string) => string): string {
  switch (status) {
    case 'Pending': return t('shopping.scanList.status.pending');
    case 'Processing': return t('shopping.scanList.status.processing');
    case 'Parsed': return t('shopping.scanList.status.parsed');
    case 'Confirmed': return t('shopping.scanList.status.confirmed');
    case 'Failed': return t('shopping.scanList.status.failed');
  }
}

export default function ReceiptScanListScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();

  const { data: corrections, isLoading, isError, refetch } = useGetCorrectionsQuery(20, {
    pollingInterval: 30000,
  });

  function statusColor(status: ParseCorrectionStatus): string {
    switch (status) {
      case 'Pending':
      case 'Processing': return colors.primary;
      case 'Parsed': return colors.secondary;
      case 'Confirmed': return colors.onSurfaceVariant;
      case 'Failed': return colors.error;
    }
  }

  function handleItemPress(item: ParseCorrectionSummaryDto) {
    if (item.status !== 'Parsed') return;
    navigation.navigate('ReceiptConfirm', { correctionId: item.id });
  }

  function renderItem({ item }: { item: ParseCorrectionSummaryDto }) {
    const isParsed = item.status === 'Parsed';
    const isActive = item.status === 'Pending' || item.status === 'Processing';
    const isFailed = item.status === 'Failed';
    const isConfirmed = item.status === 'Confirmed';

    return (
      <TouchableRipple
        onPress={() => handleItemPress(item)}
        disabled={!isParsed}
        rippleColor={colors.surfaceVariant}
        accessibilityLabel={isParsed ? t('shopping.scanList.tapToConfirm') : undefined}
      >
        <View style={[styles.card, { opacity: isConfirmed ? 0.6 : 1 }]}>
          <View style={styles.cardRow}>
            <View style={styles.cardInfo}>
              <Text variant="bodyMedium" style={{ color: colors.onSurface }}>
                {item.retailerName ?? t('shopping.scanList.unknownRetailer')}
              </Text>
              <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
                {new Intl.DateTimeFormat(undefined, {
                  month: 'short',
                  day: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit',
                }).format(new Date(item.createdAt))}
                {item.parsedItemCount > 0 ? `  ·  ${item.parsedItemCount} ${t('shopping.scanList.items')}` : ''}
              </Text>
            </View>

            <View style={styles.cardRight}>
              {isActive ? (
                <View style={styles.activeStatus}>
                  <ActivityIndicator size="small" color={colors.primary} />
                  <Text variant="labelSmall" style={{ color: colors.primary }}>
                    {statusLabel(item.status, t)}
                  </Text>
                </View>
              ) : (
                <Text
                  variant="labelSmall"
                  style={[styles.statusLabel, { color: statusColor(item.status) }]}
                >
                  {statusLabel(item.status, t)}
                </Text>
              )}
              {isParsed && (
                <Icon source="chevron-right" size={18} color={colors.secondary} />
              )}
            </View>
          </View>

          {isFailed && item.failureReason ? (
            <Text variant="bodySmall" style={{ color: colors.error, marginTop: 4 }}>
              {item.failureReason}
            </Text>
          ) : null}
        </View>
      </TouchableRipple>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {isLoading ? (
        <SkeletonList showThumb={false} />
      ) : isError ? (
        <EmptyState
          icon="alert-circle-outline"
          title={t('common.loadError')}
          action={
            <Button mode="text" onPress={refetch} icon="refresh">
              {t('common.retry')}
            </Button>
          }
        />
      ) : !corrections || corrections.length === 0 ? (
        <EmptyState
          icon="receipt-text-outline"
          title={t('shopping.scanList.emptyTitle')}
          subtitle={t('shopping.scanList.emptySubtitle')}
        />
      ) : (
        <FlatList
          data={corrections}
          keyExtractor={(c) => c.id}
          ItemSeparatorComponent={() => <Divider />}
          contentContainerStyle={styles.list}
          renderItem={renderItem}
        />
      )}

      <View style={[styles.fab, { backgroundColor: colors.surface }]}>
        <Button
          mode="contained"
          icon="qrcode-scan"
          onPress={() => navigation.navigate('ReceiptScan')}
          style={styles.scanButton}
        >
          {t('shopping.scanList.scanButton')}
        </Button>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 32 },
  list: { paddingHorizontal: 16, paddingTop: 12, paddingBottom: 100 },
  card: { paddingVertical: 12, paddingHorizontal: 16 },
  cardRow: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  cardInfo: { flex: 1, gap: 2 },
  cardRight: { flexDirection: 'row', alignItems: 'center', gap: 4, flexShrink: 0 },
  activeStatus: { flexDirection: 'row', alignItems: 'center', gap: 6 },
  statusLabel: { textAlign: 'right', flexShrink: 1 },
  fab: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: 16,
    paddingBottom: 32,
    elevation: 4,
  },
  scanButton: { width: '100%' },
});
