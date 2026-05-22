import React, { useEffect, useState } from 'react';
import { ScrollView, StyleSheet, TouchableOpacity, View } from 'react-native';
import {
  Button,
  HelperText,
  Icon,
  Text,
  TextInput,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useSelector } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { InventoryStackParamList } from '../../navigation/InventoryNavigator';
import { useAddInventoryItemMutation } from '../../store/api/inventoryApi';
import type { RootState } from '../../store';

type Props = NativeStackScreenProps<InventoryStackParamList, 'AddInventoryItem'>;

function formatDateInput(digits: string, isBrazil: boolean): string {
  // digits: raw digit chars only, up to 8
  const d = digits.slice(0, 8);
  if (isBrazil) {
    // DD/MM/YYYY
    if (d.length <= 2) return d;
    if (d.length <= 4) return `${d.slice(0, 2)}/${d.slice(2)}`;
    return `${d.slice(0, 2)}/${d.slice(2, 4)}/${d.slice(4)}`;
  }
  // MM/DD/YYYY
  if (d.length <= 2) return d;
  if (d.length <= 4) return `${d.slice(0, 2)}/${d.slice(2)}`;
  return `${d.slice(0, 2)}/${d.slice(2, 4)}/${d.slice(4)}`;
}

function parseDisplayDateToIso(display: string, isBrazil: boolean): string {
  const parts = display.split('/');
  if (parts.length !== 3) return display;
  if (isBrazil) {
    return `${parts[2]}-${parts[1]}-${parts[0]}`;
  }
  return `${parts[2]}-${parts[0]}-${parts[1]}`;
}

export default function AddInventoryItemScreen({ navigation, route }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();

  const householdCountry = useSelector((s: RootState) => s.auth.householdCountry);
  const isBrazil = householdCountry === 'BR';
  const datePlaceholder = isBrazil ? 'DD/MM/AAAA' : 'MM/DD/YYYY';

  const preselected = route.params;

  const [selectedProductId, setSelectedProductId] = useState(preselected?.preselectedProductId ?? '');
  const [selectedProductName, setSelectedProductName] = useState(preselected?.preselectedProductName ?? '');
  const [quantity, setQuantity] = useState('1');
  const [unit, setUnit] = useState(preselected?.preselectedUnit ?? '');
  const [expiryDate, setExpiryDate] = useState('');
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState('');

  const isProductLocked = !!preselected?.preselectedProductId;

  // Receive product selection back from ProductSearchScreen.
  useEffect(() => {
    const params = route.params;
    if (params?.selectedProductId && params.selectedProductId !== selectedProductId) {
      setSelectedProductId(params.selectedProductId);
      setSelectedProductName(params.selectedProductName ?? '');
      setUnit(params.selectedProductUnit ?? '');
      setErrors((prev) => ({ ...prev, product: '' }));
    }
  }, [route.params]);

  const [addItem, { isLoading }] = useAddInventoryItemMutation();

  function validate(): boolean {
    const next: Record<string, string> = {};

    if (!selectedProductId) next['product'] = t('inventory.validation.productRequired');

    const qty = parseFloat(quantity);
    if (isNaN(qty) || qty < 0) next['quantity'] = t('inventory.validation.quantityInvalid');

    if (!unit.trim()) next['unit'] = t('inventory.validation.unitRequired');

    if (expiryDate) {
      const iso = parseDisplayDateToIso(expiryDate, isBrazil);
      if (!/^\d{4}-\d{2}-\d{2}$/.test(iso)) {
        next['expiryDate'] = t('inventory.validation.expiryDateInvalid');
      } else {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (new Date(iso) < today) {
          next['expiryDate'] = t('inventory.validation.expiryDatePast');
        }
      }
    }

    setErrors(next);
    return Object.keys(next).length === 0;
  }

  async function handleSubmit() {
    if (!validate()) return;
    setServerError('');

    try {
      await addItem({
        productId: selectedProductId,
        quantity: parseFloat(quantity),
        unit: unit.trim(),
        expiryDate: expiryDate ? parseDisplayDateToIso(expiryDate, isBrazil) : null,
        notes: notes.trim() || null,
      }).unwrap();
      if (preselected?.skipToRoot) {
        navigation.popToTop();
      } else {
        navigation.goBack();
      }
    } catch {
      setServerError(t('common.error'));
    }
  }

  return (
    <ScrollView
      style={{ backgroundColor: colors.background }}
      contentContainerStyle={styles.content}
      keyboardShouldPersistTaps="handled"
    >
      {isProductLocked ? (
        <View style={[styles.lockedProduct, { backgroundColor: colors.surfaceVariant, borderRadius: 4 }]}>
          <Text variant="bodyLarge" style={{ color: colors.onSurfaceVariant }}>
            {selectedProductName}
          </Text>
        </View>
      ) : (
        <TouchableOpacity
          onPress={() => navigation.navigate('ProductSearch')}
          accessibilityRole="button"
          accessibilityLabel={t('inventory.add.searchProduct')}
        >
          <View
            style={[
              styles.productPicker,
              {
                borderColor: errors['product'] ? colors.error : colors.outline,
                borderRadius: 4,
              },
            ]}
            pointerEvents="none"
          >
            <View style={styles.productPickerInner}>
              <Text
                variant="bodySmall"
                style={[
                  styles.productPickerLabel,
                  { color: errors['product'] ? colors.error : colors.onSurfaceVariant },
                ]}
              >
                {t('inventory.add.searchProduct')}
              </Text>
              <Text
                variant="bodyLarge"
                style={{ color: selectedProductName ? colors.onSurface : colors.onSurfaceVariant }}
              >
                {selectedProductName || t('inventory.add.tapToSelect')}
              </Text>
            </View>
            <Icon source="magnify" size={20} color={colors.onSurfaceVariant} />
          </View>
        </TouchableOpacity>
      )}
      {errors['product'] ? <HelperText type="error">{errors['product']}</HelperText> : null}

      <TextInput
        mode="outlined"
        label={t('inventory.add.quantity')}
        value={quantity}
        onChangeText={setQuantity}
        keyboardType="decimal-pad"
        error={!!errors['quantity']}
      />
      {errors['quantity'] && <HelperText type="error">{errors['quantity']}</HelperText>}

      <TextInput
        mode="outlined"
        label={t('inventory.add.unit')}
        value={unit}
        onChangeText={setUnit}
        placeholder={t('inventory.add.unitPlaceholder')}
        error={!!errors['unit']}
      />
      {errors['unit'] && <HelperText type="error">{errors['unit']}</HelperText>}

      <TextInput
        mode="outlined"
        label={t('inventory.add.expiryDate')}
        value={expiryDate}
        onChangeText={(text) => {
          const digits = text.replace(/\D/g, '');
          setExpiryDate(formatDateInput(digits, isBrazil));
        }}
        placeholder={datePlaceholder}
        keyboardType="numeric"
        maxLength={10}
        error={!!errors['expiryDate']}
      />
      {errors['expiryDate'] && <HelperText type="error">{errors['expiryDate']}</HelperText>}

      <TextInput
        mode="outlined"
        label={t('inventory.add.notes')}
        value={notes}
        onChangeText={setNotes}
        placeholder={t('inventory.add.notesPlaceholder')}
        multiline
        numberOfLines={2}
      />

      {serverError ? (
        <Text style={{ color: colors.error }}>{serverError}</Text>
      ) : null}

      <View style={styles.buttonRow}>
        <Button mode="outlined" onPress={() => navigation.goBack()} style={styles.button}>
          {t('common.cancel')}
        </Button>
        <Button
          mode="contained"
          onPress={handleSubmit}
          loading={isLoading}
          disabled={isLoading}
          style={styles.button}
        >
          {t('inventory.add.submit')}
        </Button>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  content: { padding: 16, gap: 8 },
  lockedProduct: { padding: 16 },
  productPicker: {
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    paddingHorizontal: 14,
    paddingVertical: 8,
    minHeight: 56,
  },
  productPickerInner: { flex: 1, gap: 2 },
  productPickerLabel: { fontSize: 12 },
  buttonRow: { flexDirection: 'row', gap: 12, marginTop: 8 },
  button: { flex: 1 },
});
