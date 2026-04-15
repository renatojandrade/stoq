import React, { useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Badge,
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

  const { data: items, isLoading, isError, refetch } = useListInventoryQuery(
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

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (isError) {
    return (
      <View style={styles.center}>
        <Text>{t('common.error')}</Text>
        <IconButton icon="refresh" onPress={refetch} accessibilityLabel={t('common.retry')} />
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Searchbar
        placeholder={t('inventory.list.searchPlaceholder')}
        value={search}
        onChangeText={setSearch}
        style={styles.searchbar}
      />

      {items?.length === 0 ? (
        <View style={styles.center}>
          <Text variant="bodyLarge" style={{ color: colors.onSurfaceVariant }}>
            {t('inventory.list.empty')}
          </Text>
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

      <FAB
        icon="plus"
        style={[styles.fab, { backgroundColor: colors.primaryContainer }]}
        color={colors.onPrimaryContainer}
        onPress={() => navigation.navigate('AddInventoryItem')}
        accessibilityLabel={t('inventory.list.addItem')}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 8 },
  searchbar: { margin: 16 },
  list: { paddingBottom: 96 },
  actions: { flexDirection: 'row', alignItems: 'center' },
  descRow: { flexDirection: 'row', alignItems: 'center', gap: 8, flexWrap: 'wrap' },
  lowStockChip: { height: 24 },
  fab: { position: 'absolute', right: 16, bottom: 16 },
});
