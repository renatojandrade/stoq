import React, { useState } from 'react';
import { FlatList, Platform, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Chip,
  HelperText,
  Icon,
  IconButton,
  Snackbar,
  Text,
  TextInput,
  TouchableRipple,
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

// ── Background color for the list area (surface2 from the design system) ─────
const SURFACE2 = '#ECF1ED';

// ── Title-case helper (mirrors ui_kits/mobile/ReceiptConfirm.jsx titleCase) ──
// Receipt items arrive in ALL-CAPS from the OCR/NF-e parser; this makes them
// readable without discarding the user's own edits (applied only on init).
function toTitleCase(s: string): string {
  return s
    .toLowerCase()
    .replace(/\b\w/g, (m) => m.toUpperCase())
    .replace(/\d+[lL]\b/g, (m) => m.toUpperCase());   // keep "1L", "2L" uppercase
}

// ── Data types ────────────────────────────────────────────────────────────────
interface EditableItem {
  id: string;
  productName: string;
  quantity: string;
  unit: string;
  expiryDate: string;
  showExpiry: boolean;
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
    productName: toTitleCase(item.productName),
    quantity: String(item.quantity),
    unit: t(`catalog.units.${item.unit}`, { defaultValue: item.unit }),
    expiryDate: '',
    showExpiry: false,
    correctionItemId: item.correctionItemId,
    matchedProductId: item.matchedProductId,
    barcode: item.barcode,
    retailerSku: item.retailerSku,
  };
}

function isValidExpiryDate(value: string): boolean {
  if (!value) {
    return true;
  }
  return /^\d{4}-\d{2}-\d{2}$/.test(value);
}

// ── CField ─────────────────────────────────────────────────────────────────────
// Compact flat field styled to match ui_kits/mobile/ReceiptConfirm.jsx CField:
//   surface2 background · 8px radius · 2px outlineVariant underline · dense.
interface CFieldProps {
  label: string;
  value: string;
  onChangeText?: (v: string) => void;
  keyboardType?: 'default' | 'decimal-pad' | 'numeric';
  error?: boolean;
  editable?: boolean;
  style?: object;
}

function CField({ label, value, onChangeText, keyboardType, error, editable = true, style }: CFieldProps) {
  const { colors } = useTheme();
  return (
    <View style={[cfStyles.wrap, style]}>
      <TextInput
        mode="flat"
        label={label}
        value={value}
        onChangeText={onChangeText}
        keyboardType={keyboardType}
        error={error}
        editable={editable}
        dense
        style={cfStyles.input}
        underlineColor={colors.outlineVariant}
        activeUnderlineColor={colors.outlineVariant}
        underlineStyle={cfStyles.underline}
      />
    </View>
  );
}

const cfStyles = StyleSheet.create({
  wrap: {
    borderRadius: 8,
    overflow: 'hidden',
  },
  input: {
    backgroundColor: SURFACE2,
  },
  underline: {
    height: 2,
  },
});

// ── Screen ────────────────────────────────────────────────────────────────────
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

    if (validItems.length === 0) {
      return;
    }

    try {
      await batchAdd({ items: validItems, correctionId: correctionId ?? null }).unwrap();
      navigation.popToTop();
    } catch {
      setSubmitError(t('common.error'));
    }
  }

  // ── Loading / empty guards ─────────────────────────────────────────────────
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

  // ── Main render ───────────────────────────────────────────────────────────
  return (
    <View style={[styles.container, { backgroundColor: SURFACE2 }]}>

      {/* ── Summary header (primary green) ─────────────────────────────────── */}
      <View style={[styles.header, { backgroundColor: colors.primary }]}>
        {sourceRetailerName ? (
          <View style={styles.headerRow}>
            <Icon source="store-outline" size={20} color={colors.onPrimary} />
            <Text variant="titleSmall" style={[styles.headerRetailer, { color: colors.onPrimary }]}>
              {sourceRetailerName}
            </Text>
          </View>
        ) : null}
        <Text variant="bodySmall" style={[styles.headerSub, { color: colors.primaryContainer }]}>
          {alreadyConfirmed
            ? t('shopping.receipt.alreadyConfirmed')
            : `${t('shopping.receipt.itemsRecognized', { count: items.length })} · ${t('shopping.receipt.headerSubtitle')}`}
        </Text>
      </View>

      {/* ── Item cards ─────────────────────────────────────────────────────── */}
      <FlatList
        data={items}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        renderItem={({ item }) => {
          const expiryInvalid = item.expiryDate !== '' && !isValidExpiryDate(item.expiryDate);
          const isMatched = !!item.matchedProductId;

          return (
            <View style={[styles.card, { backgroundColor: colors.background }, cardShadow]}>

              {/* Status chip + remove ───────────────────────────────────── */}
              <View style={styles.cardTop}>
                {isMatched ? (
                  <Chip
                    compact
                    icon={({ size }) => (
                      <Icon source="check-circle" size={size} color={colors.onSecondaryContainer} />
                    )}
                    style={{ backgroundColor: colors.secondaryContainer }}
                    textStyle={{ color: colors.onSecondaryContainer, fontSize: 11 }}
                  >
                    {t('shopping.receipt.recognized')}
                  </Chip>
                ) : (
                  <Chip
                    compact
                    icon={({ size }) => (
                      <Icon source="help-circle-outline" size={size} color={colors.onTertiaryContainer} />
                    )}
                    style={{ backgroundColor: colors.tertiaryContainer }}
                    textStyle={{ color: colors.onTertiaryContainer, fontSize: 11 }}
                  >
                    {t('shopping.receipt.newProduct')}
                  </Chip>
                )}
                <View style={styles.spacer} />
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

              {/* Product name ───────────────────────────────────────────── */}
              <CField
                label={t('shopping.add.productName')}
                value={item.productName}
                onChangeText={(v) => updateItem(item.id, { productName: v })}
                editable={!alreadyConfirmed}
              />

              {/* Quantity (narrow) + unit (flex) ────────────────────────── */}
              <View style={styles.qtyRow}>
                <CField
                  label={t('shopping.add.quantity')}
                  value={item.quantity}
                  onChangeText={(v) => updateItem(item.id, { quantity: v })}
                  keyboardType="decimal-pad"
                  editable={!alreadyConfirmed}
                  style={styles.qtyField}
                />
                <CField
                  label={t('shopping.add.unit')}
                  value={item.unit}
                  onChangeText={(v) => updateItem(item.id, { unit: v })}
                  editable={!alreadyConfirmed}
                  style={styles.unitField}
                />
              </View>

              {/* Optional expiry ────────────────────────────────────────── */}
              {!alreadyConfirmed && (
                item.showExpiry ? (
                  <>
                    <CField
                      label={t('shopping.receipt.expiryDate')}
                      value={item.expiryDate}
                      onChangeText={(v) => updateItem(item.id, { expiryDate: v })}
                      keyboardType="numeric"
                      error={expiryInvalid}
                      editable
                    />
                    {expiryInvalid && (
                      <HelperText type="error" style={styles.helperText}>
                        {t('inventory.validation.expiryDateInvalid')}
                      </HelperText>
                    )}
                  </>
                ) : (
                  <TouchableRipple
                    onPress={() => updateItem(item.id, { showExpiry: true })}
                    style={styles.expiryTrigger}
                    rippleColor={colors.surfaceVariant}
                    accessibilityRole="button"
                  >
                    <View style={styles.expiryTriggerInner}>
                      <Icon source="calendar-plus" size={18} color={colors.primary} />
                      <Text variant="labelMedium" style={{ color: colors.primary }}>
                        {t('shopping.receipt.addExpiry')}
                      </Text>
                    </View>
                  </TouchableRipple>
                )
              )}
            </View>
          );
        }}
      />

      {/* ── Sticky footer ──────────────────────────────────────────────────── */}
      <View style={[styles.footer, { backgroundColor: colors.background }, footerShadow]}>
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
            {t('shopping.receipt.addToInventoryCount', { count: items.length })}
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

// ── Platform shadows ──────────────────────────────────────────────────────────
const cardShadow = Platform.select({
  ios: { shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.10, shadowRadius: 3 },
  android: { elevation: 2 },
  default: {},
});

const footerShadow = Platform.select({
  ios: { shadowColor: '#000', shadowOffset: { width: 0, height: -1 }, shadowOpacity: 0.08, shadowRadius: 4 },
  android: { elevation: 4 },
  default: {},
});

// ── Styles ────────────────────────────────────────────────────────────────────
const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 32 },

  // Header
  header: { paddingHorizontal: 18, paddingTop: 16, paddingBottom: 18 },
  headerRow: { flexDirection: 'row', alignItems: 'center', gap: 10 },
  headerRetailer: { flex: 1 },
  headerSub: { marginTop: 6 },

  // Card list
  list: { padding: 16, paddingTop: 14, paddingBottom: 108, gap: 12 },
  card: { borderRadius: 14, paddingTop: 14, paddingHorizontal: 14, paddingBottom: 12, gap: 10 },

  // Card internals
  cardTop: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  spacer: { flex: 1 },
  qtyRow: { flexDirection: 'row', gap: 10 },
  qtyField: { width: 96 },
  unitField: { flex: 1 },

  // Expiry affordance
  expiryTrigger: { borderRadius: 4, marginTop: 2 },
  expiryTriggerInner: { flexDirection: 'row', alignItems: 'center', gap: 8, padding: 4 },
  helperText: { marginTop: -4 },

  // Footer
  footer: { position: 'absolute', bottom: 0, left: 0, right: 0, padding: 16, paddingBottom: 32 },
  submitButton: { width: '100%' },
});
