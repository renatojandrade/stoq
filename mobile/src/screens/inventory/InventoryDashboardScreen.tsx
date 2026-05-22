import React, { useMemo, useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Chip,
  FAB,
  Icon,
  List,
  Searchbar,
  Surface,
  Text,
  TouchableRipple,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useFocusEffect } from '@react-navigation/native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { InventoryStackParamList } from '../../navigation/InventoryNavigator';
import {
  useGetExpiringItemsQuery,
  useListInventoryQuery,
  type InventoryItemDto,
} from '../../store/api/inventoryApi';

type Props = NativeStackScreenProps<InventoryStackParamList, 'InventoryDashboard'>;

const MIN_SEARCH_LENGTH = 4;

interface ProductGroup {
  productId: string;
  productName: string;
  productBrand: string | null;
  productImageUrl: string | null;
  unit: string;
  minQuantity: number;
  totalQuantity: number;
  isStockLow: boolean;
  batches: InventoryItemDto[];
}

function groupByProduct(items: InventoryItemDto[]): ProductGroup[] {
  const map = new Map<string, ProductGroup>();

  for (const item of items) {
    const existing = map.get(item.productId);
    if (existing) {
      existing.batches.push(item);
      existing.totalQuantity += item.quantity;
    } else {
      map.set(item.productId, {
        productId: item.productId,
        productName: item.productName,
        productBrand: item.productBrand,
        productImageUrl: item.productImageUrl,
        unit: item.unit,
        minQuantity: item.minQuantity,
        totalQuantity: item.quantity,
        isStockLow: false,
        batches: [item],
      });
    }
  }

  for (const group of map.values()) {
    group.isStockLow = group.minQuantity > 0 && group.totalQuantity <= group.minQuantity;
  }

  return Array.from(map.values()).sort((a, b) => a.productName.localeCompare(b.productName));
}

export default function InventoryDashboardScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [search, setSearch] = useState('');

  const activeSearch = search.length >= MIN_SEARCH_LENGTH ? search : undefined;

  const { data: items, isLoading, isError, error, refetch } = useListInventoryQuery(
    activeSearch ? { name: activeSearch } : undefined,
  );

  useFocusEffect(
    React.useCallback(() => {
      refetch();
    }, [refetch]),
  );

  const { data: expiringItems } = useGetExpiringItemsQuery(7);
  const expiringCount = expiringItems?.length ?? 0;

  const groups = useMemo(() => groupByProduct(items ?? []), [items]);

  const isExpectedEmpty =
    isError && typeof error === 'object' && error !== null && 'status' in error && error.status === 400;

  const isFetchError =
    isError && typeof error === 'object' && error !== null && 'status' in error && error.status === 'FETCH_ERROR';

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (isError && !isExpectedEmpty) {
    return (
      <View style={styles.center}>
        <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant }}>
          {isFetchError ? t('common.networkError') : t('common.error')}
        </Text>
        <Button mode="text" onPress={refetch} icon="refresh">
          {t('common.retry')}
        </Button>
      </View>
    );
  }

  const isEmpty = isExpectedEmpty || groups.length === 0;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <TouchableRipple
        onPress={() => navigation.navigate('ExpiryAlerts')}
        accessibilityLabel={t('inventory.expiry.title')}
      >
        <Surface
          style={[
            styles.expiryBanner,
            { backgroundColor: expiringCount > 0 ? colors.errorContainer : colors.surfaceVariant },
          ]}
          elevation={0}
        >
          <Icon
            source="calendar-alert"
            size={20}
            color={expiringCount > 0 ? colors.onErrorContainer : colors.onSurfaceVariant}
          />
          <Text
            variant="labelLarge"
            style={[
              styles.expiryBannerText,
              { color: expiringCount > 0 ? colors.onErrorContainer : colors.onSurfaceVariant },
            ]}
          >
            {expiringCount > 0
              ? t('inventory.expiry.banner', { count: expiringCount })
              : t('inventory.expiry.bannerEmpty')}
          </Text>
          <Icon
            source="chevron-right"
            size={20}
            color={expiringCount > 0 ? colors.onErrorContainer : colors.onSurfaceVariant}
          />
        </Surface>
      </TouchableRipple>

      {!isEmpty && (
        <Searchbar
          placeholder={t('inventory.list.searchPlaceholder')}
          value={search}
          onChangeText={setSearch}
          style={styles.searchbar}
        />
      )}

      {isEmpty ? (
        <View style={styles.center}>
          <List.Icon icon="fridge-outline" color={colors.onSurfaceVariant} />
          <Text variant="titleMedium" style={{ color: colors.onSurface }}>
            {t('inventory.list.emptyTitle')}
          </Text>
          <Text
            variant="bodyMedium"
            style={[styles.emptySubtitle, { color: colors.onSurfaceVariant }]}
          >
            {t('inventory.list.emptySubtitle')}
          </Text>
          <Button
            mode="contained"
            icon="plus"
            onPress={() => navigation.navigate('AddInventoryItem')}
            style={styles.emptyButton}
          >
            {t('inventory.list.addFirstItem')}
          </Button>
        </View>
      ) : (
        <FlatList
          data={groups}
          keyExtractor={(group) => group.productId}
          renderItem={({ item: group }) => {
            const unitLabel = t(`catalog.units.${group.unit}`, { defaultValue: group.unit });
            return (
              <TouchableRipple
                onPress={() =>
                  navigation.navigate('ProductInventoryDetail', {
                    productId: group.productId,
                    productName: group.productName,
                    productBrand: group.productBrand,
                    productImageUrl: group.productImageUrl,
                    unit: group.unit,
                    minQuantity: group.minQuantity,
                  })
                }
                rippleColor={colors.surfaceVariant}
              >
                <View style={styles.row}>
                  <View style={[styles.iconWrap, { backgroundColor: colors.surfaceVariant }]}>
                    <Icon source="fridge-outline" size={22} color={colors.onSurfaceVariant} />
                  </View>
                  <View style={styles.rowContent}>
                    <Text
                      variant="bodyLarge"
                      style={{ color: colors.onSurface }}
                      numberOfLines={2}
                    >
                      {group.productName}
                    </Text>
                    <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
                      {`${group.totalQuantity} ${unitLabel}`}
                      {group.batches.length > 1
                        ? `  ·  ${t('inventory.list.batchCount', { count: group.batches.length })}`
                        : ''}
                    </Text>
                    {group.isStockLow && (
                      <Chip
                        compact
                        style={[styles.lowStockChip, { backgroundColor: colors.errorContainer }]}
                        textStyle={{ color: colors.onErrorContainer, fontSize: 11 }}
                      >
                        {t('inventory.list.lowStock')}
                      </Chip>
                    )}
                  </View>
                  <Icon source="chevron-right" size={20} color={colors.onSurfaceVariant} />
                </View>
              </TouchableRipple>
            );
          }}
          contentContainerStyle={styles.list}
        />
      )}

      {!isEmpty && (
        <FAB
          icon="plus"
          style={[styles.fab, { backgroundColor: colors.primaryContainer }]}
          color={colors.onPrimaryContainer}
          onPress={() => navigation.navigate('AddInventoryItem')}
          accessibilityLabel={t('inventory.list.addItem')}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12, paddingHorizontal: 32 },
  expiryBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
    gap: 10,
  },
  expiryBannerText: { flex: 1 },
  searchbar: { margin: 16 },
  list: { paddingBottom: 96 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 12,
    gap: 12,
  },
  iconWrap: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: 'center',
    justifyContent: 'center',
  },
  rowContent: { flex: 1, gap: 3 },
  lowStockChip: { alignSelf: 'flex-start', marginTop: 2 },
  fab: { position: 'absolute', right: 16, bottom: 16 },
  emptySubtitle: { textAlign: 'center' },
  emptyButton: { marginTop: 8 },
});
