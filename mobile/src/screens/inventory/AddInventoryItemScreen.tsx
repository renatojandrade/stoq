import React, { useState } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import {
  Button,
  HelperText,
  Text,
  TextInput,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { InventoryStackParamList } from '../../navigation/InventoryNavigator';
import { useListProductsQuery } from '../../store/api/catalogApi';
import { useAddInventoryItemMutation } from '../../store/api/inventoryApi';

type Props = NativeStackScreenProps<InventoryStackParamList, 'AddInventoryItem'>;

export default function AddInventoryItemScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();

  const [productSearch, setProductSearch] = useState('');
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedProductName, setSelectedProductName] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [unit, setUnit] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState('');

  const { data: products } = useListProductsQuery(
    productSearch ? { name: productSearch } : undefined,
    { skip: !productSearch },
  );

  const [addItem, { isLoading }] = useAddInventoryItemMutation();

  function selectProduct(id: string, name: string, productUnit: string) {
    setSelectedProductId(id);
    setSelectedProductName(name);
    setUnit(productUnit);
    setProductSearch('');
  }

  function validate(): boolean {
    const next: Record<string, string> = {};

    if (!selectedProductId) next['product'] = t('inventory.validation.productRequired');

    const qty = parseFloat(quantity);
    if (isNaN(qty) || qty < 0) next['quantity'] = t('inventory.validation.quantityInvalid');

    if (!unit.trim()) next['unit'] = t('inventory.validation.unitRequired');

    if (expiryDate && !/^\d{4}-\d{2}-\d{2}$/.test(expiryDate)) {
      next['expiryDate'] = t('inventory.validation.expiryDateInvalid');
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
        expiryDate: expiryDate || null,
        notes: notes.trim() || null,
      }).unwrap();
      navigation.goBack();
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
      <TextInput
        mode="outlined"
        label={t('inventory.add.searchProduct')}
        value={selectedProductId ? selectedProductName : productSearch}
        onChangeText={(text) => {
          if (selectedProductId) {
            setSelectedProductId('');
            setSelectedProductName('');
          }
          setProductSearch(text);
        }}
        right={
          selectedProductId ? (
            <TextInput.Icon
              icon="close-circle"
              onPress={() => {
                setSelectedProductId('');
                setSelectedProductName('');
                setProductSearch('');
              }}
            />
          ) : undefined
        }
        error={!!errors['product']}
      />
      {errors['product'] && <HelperText type="error">{errors['product']}</HelperText>}

      {!selectedProductId && products && products.length > 0 && (
        <View style={[styles.suggestions, { backgroundColor: colors.surface, borderColor: colors.outline }]}>
          {products.slice(0, 5).map((p) => (
            <Button
              key={p.id}
              mode="text"
              onPress={() => selectProduct(p.id, p.name, p.unit)}
              style={styles.suggestion}
              contentStyle={styles.suggestionContent}
            >
              {`${p.name}${p.brand ? ` · ${p.brand}` : ''}`}
            </Button>
          ))}
        </View>
      )}

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
        onChangeText={setExpiryDate}
        placeholder="YYYY-MM-DD"
        keyboardType="numeric"
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
  suggestions: { borderWidth: 1, borderRadius: 4 },
  suggestion: { justifyContent: 'flex-start' },
  suggestionContent: { justifyContent: 'flex-start' },
  buttonRow: { flexDirection: 'row', gap: 12, marginTop: 8 },
  button: { flex: 1 },
});
