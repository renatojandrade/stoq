import React, { useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Card,
  Chip,
  Divider,
  FAB,
  IconButton,
  Snackbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useSelector } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { InventoryStackParamList } from '../../navigation/InventoryNavigator';
import {
  useListInventoryQuery,
  useAddInventoryItemMutation,
  useConsumeInventoryItemMutation,
  useRemoveInventoryItemMutation,
  type InventoryItemDto,
} from '../../store/api/inventoryApi';
import { useAddShoppingItemMutation } from '../../store/api/shoppingApi';
import type { RootState } from '../../store';

type Props = NativeStackScreenProps<InventoryStackParamList, 'ProductInventoryDetail'>;

function formatIsoDate(iso: string, isBrazil: boolean): string {
  const [year, month, day] = iso.split('-');
  if (!year || !month || !day) return iso;
  return isBrazil ? `${day}/${month}/${year}` : `${month}/${day}/${year}`;
}

export default function ProductInventoryDetailScreen({ route, navigation }: Props) {
  const { productId, productName, productBrand, unit, minQuantity } = route.params;
  const { t } = useTranslation();
  const { colors } = useTheme();

  const householdCountry = useSelector((s: RootState) => s.auth.householdCountry);
  const isBrazil = householdCountry === 'BR';

  const { data: batches, isLoading, isError, refetch } = useListInventoryQuery(
    { productId },
    { refetchOnFocus: true },
  );

  const [addItem] = useAddInventoryItemMutation();
  const [consumeItem] = useConsumeInventoryItemMutation();
  const [removeItem] = useRemoveInventoryItemMutation();
  const [addShoppingItem] = useAddShoppingItemMutation();
  const [snackbarVisible, setSnackbarVisible] = useState(false);

  async function handleAddOne(item: InventoryItemDto) {
    await addItem({ productId: item.productId, quantity: 1, unit: item.unit, expiryDate: item.expiryDate });
  }

  async function handleConsumeOne(item: InventoryItemDto) {
    await consumeItem({ id: item.id, amount: 1 });
  }

  async function handleRemove(id: string) {
    await removeItem(id);
  }

  async function handleAddToShoppingList() {
    await addShoppingItem({ productName, quantity: 1, unit });
    setSnackbarVisible(true);
  }

  const totalQuantity = batches?.reduce((sum, b) => sum + b.quantity, 0) ?? 0;
  const isStockLow = minQuantity > 0 && totalQuantity <= minQuantity;

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
        <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant }}>
          {t('common.error')}
        </Text>
        <Button mode="text" onPress={refetch} icon="refresh">
          {t('common.retry')}
        </Button>
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Card style={styles.summaryCard}>
        <Card.Content style={styles.summaryContent}>
          <View style={styles.summaryRow}>
            <Text variant="headlineSmall" style={{ color: colors.onSurface }}>
              {`${totalQuantity} ${t(`catalog.units.${unit}`, { defaultValue: unit })}`}
            </Text>
            {isStockLow && (
              <Chip
                compact
                style={{ backgroundColor: colors.errorContainer }}
                textStyle={{ color: colors.onErrorContainer }}
              >
                {t('inventory.list.lowStock')}
              </Chip>
            )}
          </View>
          {productBrand ? (
            <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant }}>
              {productBrand}
            </Text>
          ) : null}
          <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
            {t('inventory.detail.minStock', { quantity: minQuantity, unit: t(`catalog.units.${unit}`, { defaultValue: unit }) })}
          </Text>
          <Button
            mode="outlined"
            icon="cart-plus"
            onPress={handleAddToShoppingList}
            style={styles.shoppingButton}
            compact
          >
            {t('inventory.detail.addToShoppingList')}
          </Button>
        </Card.Content>
      </Card>

      <Text variant="titleSmall" style={[styles.batchesTitle, { color: colors.onSurfaceVariant }]}>
        {t('inventory.detail.batchesTitle')}
      </Text>

      <FlatList
        data={batches}
        keyExtractor={(item) => item.id}
        ItemSeparatorComponent={() => <Divider />}
        renderItem={({ item }) => (
          <View style={[styles.batchRow, { backgroundColor: colors.surface }]}>
            <View style={styles.batchInfo}>
              <Text variant="bodyLarge" style={{ color: colors.onSurface }}>
                {`${item.quantity} ${t(`catalog.units.${item.unit}`, { defaultValue: item.unit })}`}
              </Text>
              {item.expiryDate ? (
                <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
                  {t('inventory.list.expires')} {formatIsoDate(item.expiryDate, isBrazil)}
                </Text>
              ) : (
                <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
                  {t('inventory.detail.noExpiry')}
                </Text>
              )}
              {item.notes ? (
                <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
                  {item.notes}
                </Text>
              ) : null}
            </View>
            <View style={styles.batchActions}>
              <IconButton
                icon="minus-circle-outline"
                size={22}
                onPress={() => handleConsumeOne(item)}
                accessibilityLabel={t('inventory.list.consume')}
              />
              <IconButton
                icon="plus-circle-outline"
                size={22}
                onPress={() => handleAddOne(item)}
                accessibilityLabel={t('inventory.detail.addOne')}
              />
              <IconButton
                icon="trash-can-outline"
                size={22}
                iconColor={colors.error}
                onPress={() => handleRemove(item.id)}
                accessibilityLabel={t('inventory.list.remove')}
              />
            </View>
          </View>
        )}
        contentContainerStyle={styles.list}
      />

      <FAB
        icon="plus"
        label={t('inventory.detail.addBatch')}
        style={[styles.fab, { backgroundColor: colors.primaryContainer }]}
        color={colors.onPrimaryContainer}
        onPress={() =>
          navigation.navigate('AddInventoryItem', {
            preselectedProductId: productId,
            preselectedProductName: productName,
            preselectedUnit: unit,
          })
        }
        accessibilityLabel={t('inventory.detail.addBatch')}
      />

      <Snackbar
        visible={snackbarVisible}
        onDismiss={() => setSnackbarVisible(false)}
        duration={3000}
        action={{ label: t('common.ok'), onPress: () => setSnackbarVisible(false) }}
      >
        {t('inventory.detail.addedToShoppingList')}
      </Snackbar>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12, paddingHorizontal: 32 },
  summaryCard: { margin: 16 },
  summaryContent: { gap: 4 },
  shoppingButton: { marginTop: 8, alignSelf: 'flex-start' },
  summaryRow: { flexDirection: 'row', alignItems: 'center', gap: 12, flexWrap: 'wrap' },
  batchesTitle: { paddingHorizontal: 16, paddingBottom: 8, textTransform: 'uppercase', letterSpacing: 0.5 },
  list: { paddingBottom: 96 },
  batchRow: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 16, paddingVertical: 12 },
  batchInfo: { flex: 1, gap: 2 },
  batchActions: { flexDirection: 'row', alignItems: 'center' },
  fab: { position: 'absolute', right: 16, bottom: 16 },
});
