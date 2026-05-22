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
import type { ShoppingStackParamList } from '../../navigation/ShoppingNavigator';
import { useAddShoppingItemMutation } from '../../store/api/shoppingApi';

type Props = NativeStackScreenProps<ShoppingStackParamList, 'AddShoppingItem'>;

export default function AddShoppingItemScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();

  const [productName, setProductName] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [unit, setUnit] = useState('');
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const [addItem, { isLoading }] = useAddShoppingItemMutation();

  function validate(): boolean {
    const next: Record<string, string> = {};

    if (!productName.trim()) {
      next['productName'] = t('shopping.validation.productNameRequired');
    }

    const qty = parseFloat(quantity);
    if (isNaN(qty) || qty <= 0) {
      next['quantity'] = t('shopping.validation.quantityInvalid');
    }

    if (!unit.trim()) {
      next['unit'] = t('shopping.validation.unitRequired');
    }

    setErrors(next);
    return Object.keys(next).length === 0;
  }

  async function handleSubmit() {
    if (!validate()) {
      return;
    }

    try {
      await addItem({
        productName: productName.trim(),
        quantity: parseFloat(quantity),
        unit: unit.trim(),
        notes: notes.trim() || null,
      }).unwrap();

      navigation.goBack();
    } catch {
      setErrors({ submit: t('common.error') });
    }
  }

  return (
    <ScrollView
      style={[styles.container, { backgroundColor: colors.background }]}
      contentContainerStyle={styles.content}
      keyboardShouldPersistTaps="handled"
    >
      <TextInput
        mode="outlined"
        label={t('shopping.add.productName')}
        value={productName}
        onChangeText={setProductName}
        autoFocus
        error={!!errors['productName']}
      />
      {errors['productName'] ? (
        <HelperText type="error">{errors['productName']}</HelperText>
      ) : null}

      <View style={styles.row}>
        <View style={styles.quantityInput}>
          <TextInput
            mode="outlined"
            label={t('shopping.add.quantity')}
            value={quantity}
            onChangeText={setQuantity}
            keyboardType="decimal-pad"
            error={!!errors['quantity']}
          />
          {errors['quantity'] ? (
            <HelperText type="error">{errors['quantity']}</HelperText>
          ) : null}
        </View>
        <View style={styles.unitInput}>
          <TextInput
            mode="outlined"
            label={t('shopping.add.unit')}
            value={unit}
            onChangeText={setUnit}
            error={!!errors['unit']}
          />
          {errors['unit'] ? (
            <HelperText type="error">{errors['unit']}</HelperText>
          ) : null}
        </View>
      </View>

      <TextInput
        mode="outlined"
        label={t('shopping.add.notes')}
        value={notes}
        onChangeText={setNotes}
        placeholder={t('shopping.add.notesPlaceholder')}
        multiline
        numberOfLines={2}
      />

      {errors['submit'] ? (
        <Text variant="bodySmall" style={{ color: colors.error, marginTop: 8 }}>
          {errors['submit']}
        </Text>
      ) : null}

      <Button
        mode="contained"
        onPress={handleSubmit}
        loading={isLoading}
        disabled={isLoading}
        style={styles.submitButton}
      >
        {t('shopping.add.submit')}
      </Button>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  content: { padding: 16, gap: 12 },
  row: { flexDirection: 'row', gap: 12 },
  quantityInput: { flex: 1 },
  unitInput: { flex: 1 },
  submitButton: { marginTop: 8 },
});
