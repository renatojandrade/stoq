import React, { useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Chip,
  FAB,
  IconButton,
  List,
  Searchbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { InventoryStackParamList } from '../../navigation/InventoryNavigator';
import {
  useListInventoryQuery,
  useRemoveInventoryItemMutation,
  type InventoryItemDto,
} from '../../store/api/inventoryApi';

type Props = NativeStackScreenProps<InventoryStackParamList, 'InventoryDashboard'>;

export default function InventoryDashboardScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [search, setSearch] = useState('');

  const { data: items, isLoading, isError, error, refetch } = useListInventoryQuery(
    search ? { name: search } : undefined,
    { refetchOnFocus: true },
  );

  const [removeItem] = useRemoveInventoryItemMutation();

  async function handleRemove(id: string) {
    await removeItem(id);
  }

  function handleConsume(item: InventoryItemDto) {
    navigation.navigate('ConsumeInventoryItem', { item });
  }

  // A 400 means the JWT doesn't carry householdId yet (e.g. right after household
  // creation before a token refresh). Treat it like an empty list rather than a
  // hard error so the user sees the onboarding prompt instead of "something went wrong".
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

  const isEmpty = isExpectedEmpty || items?.length === 0;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
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
          data={items}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <List.Item
              title={item.productName}
              description={() => (
                <View style={styles.descRow}>
                  <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
                    {`${item.quantity} ${item.unit}`}
                    {item.expiryDate
                      ? `  ·  ${t('inventory.list.expires')} ${item.expiryDate}`
                      : ''}
                  </Text>
                  {item.isStockLow && (
                    <Chip
                      compact
                      style={[styles.lowStockChip, { backgroundColor: colors.errorContainer }]}
                      textStyle={{ color: colors.onErrorContainer }}
                    >
                      {t('inventory.list.lowStock')}
                    </Chip>
                  )}
                </View>
              )}
              left={(props) => <List.Icon {...props} icon="fridge-outline" />}
              right={() => (
                <View style={styles.actions}>
                  <IconButton
                    icon="minus-circle-outline"
                    size={20}
                    onPress={() => handleConsume(item)}
                    accessibilityLabel={t('inventory.list.consume')}
                  />
                  <IconButton
                    icon="trash-can-outline"
                    size={20}
                    iconColor={colors.error}
                    onPress={() => handleRemove(item.id)}
                    accessibilityLabel={t('inventory.list.remove')}
                  />
                </View>
              )}
            />
          )}
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
  searchbar: { margin: 16 },
  list: { paddingBottom: 96 },
  actions: { flexDirection: 'row', alignItems: 'center' },
  descRow: { flexDirection: 'row', alignItems: 'center', gap: 8, flexWrap: 'wrap' },
  lowStockChip: { height: 24 },
  fab: { position: 'absolute', right: 16, bottom: 16 },
  emptySubtitle: { textAlign: 'center' },
  emptyButton: { marginTop: 8 },
});
