import React, { useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Divider,
  HelperText,
  IconButton,
  Snackbar,
  Text,
  TextInput,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { ShoppingStackParamList } from '../../navigation/ShoppingNavigator';
import {
  useBatchAddInventoryItemsMutation,
  useGetCorrectionDetailQuery,
  type BatchInventoryItemRequest,
  type ParsedReceiptItemDto,
} from '../../store/api/receiptsApi';

type Props = NativeStackScreenProps<ShoppingStackParamList, 'ReceiptConfirm'>;

interface EditableItem {
  id: string;
  productName: string;
  quantity: string;
  unit: string;
  expiryDate: string;
  correctionItemId?: string;
  matchedProductId?: string | null;
  barcode?: string | null;
  retailerSku?: string | null;
}

function toEditable(
  item: ParsedReceiptItemDto,
  index: number,
  t: (key: string, opts?: { defaultValue: string }) => string,
): EditableItem {
  return {
    id: `${index}`,
    productName: item.productName,
    quantity: String(item.quantity),
    unit: t(`catalog.units.${item.unit}`, { defaultValue: item.unit }),
    expiryDate: '',
    correctionItemId: item.correctionItemId,
    matchedProductId: item.matchedProductId,
    barcode: item.barcode,
    retailerSku: item.retailerSku,
  };
}

function isValidExpiryDate(value: string): boolean {
  if (!value) return true;
  return /^\d{4}-\d{2}-\d{2}$/.test(value);
}

export default function ReceiptConfirmScreen({ navigation, route }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const params = route.params;

  const isNfeMode = 'correctionId' in params && params.correctionId != null;
  const correctionId = isNfeMode ? params.correctionId : undefined;
  const photoItems = isNfeMode ? undefined : params.items;
  const retailerName = isNfeMode ? undefined : params.retailerName;

  const { data: detail, isLoading: isDetailLoading } = useGetCorrectionDetailQuery(
    correctionId!,
    { skip: !correctionId },
  );

  const sourceItems = isNfeMode ? detail?.items : photoItems;
  const sourceRetailerName = isNfeMode ? detail?.retailerName : retailerName;
  const alreadyConfirmed = isNfeMode && detail?.status === 'Confirmed';

  const [items, setItems] = useState<EditableItem[]>([]);
  const [initialised, setInitialised] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [batchAdd, { isLoading }] = useBatchAddInventoryItemsMutation();

  if (!initialised && sourceItems && sourceItems.length > 0) {
    setItems(sourceItems.map((item, index) => toEditable(item, index, t)));
    setInitialised(true);
  }

  function updateItem(id: string, patch: Partial<EditableItem>) {
    setItems((prev) => prev.map((i) => (i.id === id ? { ...i, ...patch } : i)));
  }

  function removeItem(id: string) {
    setItems((prev) => prev.filter((i) => i.id !== id));
  }

  async function handleConfirm() {
    const validItems: BatchInventoryItemRequest[] = items
      .filter((i) => i.productName.trim() && Number(i.quantity) > 0 && i.unit.trim())
      .map((i) => ({
        productName: i.productName.trim(),
        quantity: Number(i.quantity),
        unit: i.unit.trim(),
        expiryDate: isValidExpiryDate(i.expiryDate) && i.expiryDate ? i.expiryDate : null,
        notes: null,
        correctionItemId: i.correctionItemId ?? null,
        matchedProductId: i.matchedProductId ?? null,
        barcode: i.barcode ?? null,
        retailerSku: i.retailerSku ?? null,
      }));

    if (validItems.length === 0) return;

    try {
      await batchAdd({ items: validItems, correctionId: correctionId ?? null }).unwrap();
      navigation.popToTop();
    } catch {
      setSubmitError(t('common.error'));
    }
  }

  if (isDetailLoading) {
    return (
      <View style={[styles.center, { backgroundColor: colors.background }]}>
        <ActivityIndicator />
      </View>
    );
  }

  if (items.length === 0 && !isDetailLoading) {
    return (
      <View style={[styles.center, { backgroundColor: colors.background }]}>
        <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant, textAlign: 'center' }}>
          {t('shopping.receipt.confirmEmpty')}
        </Text>
        <Button mode="contained" onPress={() => navigation.goBack()} style={{ marginTop: 12 }}>
          {t('common.retry')}
        </Button>
      </View>
    );
  }

  const canSubmit =
    !isLoading &&
    !alreadyConfirmed &&
    items.some(
      (i) =>
        i.productName.trim() &&
        Number(i.quantity) > 0 &&
        i.unit.trim() &&
        isValidExpiryDate(i.expiryDate),
    );

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {sourceRetailerName ? (
        <Text variant="titleSmall" style={[styles.retailerName, { color: colors.primary }]}>
          {sourceRetailerName}
        </Text>
      ) : null}
      <Text variant="bodySmall" style={[styles.subtitle, { color: colors.onSurfaceVariant }]}>
        {alreadyConfirmed
          ? t('shopping.receipt.alreadyConfirmed')
          : t('shopping.receipt.confirmSubtitle')}
      </Text>

      <FlatList
        data={items}
        keyExtractor={(item) => item.id}
        ItemSeparatorComponent={() => <Divider />}
        contentContainerStyle={styles.list}
        renderItem={({ item }) => {
          const expiryInvalid = item.expiryDate !== '' && !isValidExpiryDate(item.expiryDate);
          return (
            <View style={styles.card}>
              <View style={styles.cardHeader}>
                <TextInput
                  mode="outlined"
                  label={t('shopping.add.productName')}
                  value={item.productName}
                  onChangeText={(v) => updateItem(item.id, { productName: v })}
                  style={styles.nameInput}
                  dense
                  editable={!alreadyConfirmed}
                />
                {!alreadyConfirmed && (
                  <IconButton
                    icon="close"
                    size={18}
                    iconColor={colors.onSurfaceVariant}
                    onPress={() => removeItem(item.id)}
                    accessibilityLabel={t('shopping.receipt.removeItem')}
                  />
                )}
              </View>

              <View style={styles.row}>
                <TextInput
                  mode="outlined"
                  label={t('shopping.add.quantity')}
                  value={item.quantity}
                  onChangeText={(v) => updateItem(item.id, { quantity: v })}
                  keyboardType="decimal-pad"
                  style={styles.quantityInput}
                  dense
                  editable={!alreadyConfirmed}
                />
                <TextInput
                  mode="outlined"
                  label={t('shopping.add.unit')}
                  value={item.unit}
                  onChangeText={(v) => updateItem(item.id, { unit: v })}
                  style={styles.unitInput}
                  dense
                  editable={!alreadyConfirmed}
                />
              </View>

              {!alreadyConfirmed && (
                <>
                  <TextInput
                    mode="outlined"
                    label={t('shopping.receipt.expiryDate')}
                    placeholder={t('shopping.receipt.expiryPlaceholder')}
                    value={item.expiryDate}
                    onChangeText={(v) => updateItem(item.id, { expiryDate: v })}
                    keyboardType="numeric"
                    style={styles.expiryInput}
                    dense
                    error={expiryInvalid}
                  />
                  {expiryInvalid && (
                    <HelperText type="error">
                      {t('inventory.validation.expiryDateInvalid')}
                    </HelperText>
                  )}
                </>
              )}
            </View>
          );
        }}
      />

      <View style={[styles.footer, { backgroundColor: colors.surface }]}>
        {isLoading ? (
          <ActivityIndicator />
        ) : (
          <Button
            mode="contained"
            onPress={handleConfirm}
            disabled={!canSubmit}
            icon="check"
            style={styles.submitButton}
          >
            {t('shopping.receipt.addToInventory')}
          </Button>
        )}
      </View>

      <Snackbar
        visible={submitError !== null}
        onDismiss={() => setSubmitError(null)}
        duration={4000}
        action={{ label: t('common.ok'), onPress: () => setSubmitError(null) }}
      >
        {submitError ?? ''}
      </Snackbar>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 32 },
  retailerName: { marginHorizontal: 16, marginTop: 16, marginBottom: 2 },
  subtitle: { marginHorizontal: 16, marginBottom: 8 },
  list: { paddingHorizontal: 16, paddingBottom: 100 },
  card: { paddingVertical: 12, gap: 8 },
  cardHeader: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  nameInput: { flex: 1 },
  row: { flexDirection: 'row', gap: 8 },
  quantityInput: { flex: 1 },
  unitInput: { flex: 1 },
  expiryInput: {},
  footer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: 16,
    paddingBottom: 32,
    elevation: 4,
  },
  submitButton: { width: '100%' },
});
