import React, { useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  Button,
  Checkbox,
  Chip,
  Divider,
  FAB,
  Text,
  TouchableRipple,
  IconButton,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useFocusEffect } from '@react-navigation/native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { ShoppingStackParamList } from '../../navigation/ShoppingNavigator';
import ProductThumb from '../../components/ProductThumb';
import SkeletonList from '../../components/SkeletonList';
import EmptyState from '../../components/EmptyState';
import {
  useListShoppingQuery,
  useCheckOffItemMutation,
  useUncheckItemMutation,
  useRemoveShoppingItemMutation,
  useClearCheckedItemsMutation,
  type ShoppingListItemDto,
} from '../../store/api/shoppingApi';

type Props = NativeStackScreenProps<ShoppingStackParamList, 'ShoppingList'>;

export default function ShoppingListScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [fabOpen, setFabOpen] = useState(false);

  const { data: items, isLoading, isError, error, refetch } = useListShoppingQuery(undefined, {
    refetchOnFocus: true,
  });

  useFocusEffect(
    React.useCallback(() => {
      refetch();
    }, [refetch]),
  );

  const [checkOff] = useCheckOffItemMutation();
  const [uncheck] = useUncheckItemMutation();
  const [removeItem] = useRemoveShoppingItemMutation();
  const [clearChecked] = useClearCheckedItemsMutation();

  const isFetchError =
    isError && typeof error === 'object' && error !== null && 'status' in error && error.status === 'FETCH_ERROR';

  if (isLoading) {
    return <SkeletonList />;
  }

  if (isError) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <EmptyState
          icon="alert-circle-outline"
          title={isFetchError ? t('common.networkError') : t('common.loadError')}
          action={
            <Button mode="text" onPress={refetch} icon="refresh">
              {t('common.retry')}
            </Button>
          }
        />
      </View>
    );
  }

  const isEmpty = !items || items.length === 0;
  const hasChecked = items?.some((i) => i.isChecked) ?? false;
  const unchecked = items?.filter((i) => !i.isChecked) ?? [];
  const checked = items?.filter((i) => i.isChecked) ?? [];

  async function handleToggle(item: ShoppingListItemDto) {
    if (item.isChecked) {
      await uncheck(item.id);
    } else {
      await checkOff(item.id);
    }
  }

  async function handleRemove(id: string) {
    await removeItem(id);
  }

  async function handleClearChecked() {
    await clearChecked();
  }

  function renderItem({ item }: { item: ShoppingListItemDto }) {
    const unitLabel = t(`catalog.units.${item.unit}`, { defaultValue: item.unit });
    return (
      <TouchableRipple onPress={() => handleToggle(item)} rippleColor={colors.surfaceVariant}>
        <View style={[styles.row, { backgroundColor: colors.surface }]}>
          <Checkbox
            status={item.isChecked ? 'checked' : 'unchecked'}
            onPress={() => handleToggle(item)}
          />
          <ProductThumb name={item.productName} imageUrl={null} size={40} />
          <View style={styles.rowContent}>
            <Text
              variant="bodyLarge"
              style={[{ color: colors.onSurface }, item.isChecked && styles.strikethrough]}
              numberOfLines={2}
            >
              {item.productName}
            </Text>
            <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
              {`${item.quantity} ${unitLabel}`}
            </Text>
            {item.source === 'Auto' && (
              <Chip
                compact
                style={[styles.autoChip, { backgroundColor: colors.secondaryContainer }]}
                textStyle={{ color: colors.onSecondaryContainer, fontSize: 11 }}
              >
                {t('shopping.list.autoAdded')}
              </Chip>
            )}
            {item.notes ? (
              <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
                {item.notes}
              </Text>
            ) : null}
          </View>
          <IconButton
            icon="trash-can-outline"
            size={20}
            iconColor={colors.onSurfaceVariant}
            onPress={() => handleRemove(item.id)}
            accessibilityLabel={t('shopping.list.remove')}
          />
        </View>
      </TouchableRipple>
    );
  }

  if (isEmpty) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <EmptyState
          icon="cart-outline"
          title={t('shopping.list.emptyTitle')}
          subtitle={t('shopping.list.emptySubtitle')}
          action={
            <>
              <Button
                mode="contained"
                icon="plus"
                onPress={() => navigation.navigate('AddShoppingItem')}
                style={styles.emptyButton}
              >
                {t('shopping.list.addFirstItem')}
              </Button>
              <Button
                mode="outlined"
                icon="receipt-text-outline"
                onPress={() => navigation.navigate('ReceiptScanList')}
                style={styles.emptyButton}
              >
                {t('shopping.list.scanReceipt')}
              </Button>
            </>
          }
        />
      </View>
    );
  }

  const seen = new Set<string>();
  const allItems = [...unchecked, ...checked].filter((i) => {
    if (seen.has(i.id)) return false;
    seen.add(i.id);
    return true;
  });

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {hasChecked && (
        <Button
          mode="text"
          icon="check-all"
          onPress={handleClearChecked}
          style={styles.clearButton}
          textColor={colors.error}
        >
          {t('shopping.list.clearChecked')}
        </Button>
      )}

      <FlatList
        data={allItems}
        keyExtractor={(item) => item.id}
        ItemSeparatorComponent={() => <Divider />}
        renderItem={renderItem}
        contentContainerStyle={styles.list}
      />

      <FAB.Group
        open={fabOpen}
        visible
        icon={fabOpen ? 'close' : 'plus'}
        style={styles.fabGroup}
        actions={[
          {
            icon: 'receipt-text-outline',
            label: t('shopping.list.scanReceipt'),
            onPress: () => navigation.navigate('ReceiptScanList'),
            accessibilityLabel: t('shopping.list.scanReceipt'),
          },
          {
            icon: 'plus',
            label: t('shopping.list.addItem'),
            onPress: () => navigation.navigate('AddShoppingItem'),
            accessibilityLabel: t('shopping.list.addItem'),
          },
        ]}
        onStateChange={({ open }) => setFabOpen(open)}
        accessibilityLabel={t('shopping.list.addItem')}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12, paddingHorizontal: 32, paddingTop: 0 },
  clearButton: { alignSelf: 'flex-end', marginRight: 8, marginTop: 4 },
  list: { paddingBottom: 96 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingLeft: 14,
    paddingRight: 8,
    paddingVertical: 10,
    gap: 12,
  },
  rowContent: { flex: 1, gap: 3, paddingVertical: 4 },
  autoChip: { alignSelf: 'flex-start', marginTop: 2 },
  strikethrough: { textDecorationLine: 'line-through', opacity: 0.5 },
  emptyButton: { width: '100%' },
  fabGroup: { position: 'absolute', right: 0, bottom: 0 },
});
