import React, { useState } from 'react';
import { StyleSheet, View } from 'react-native';
import {
  Button,
  Card,
  HelperText,
  Text,
  TextInput,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { InventoryStackParamList } from '../../navigation/InventoryNavigator';
import { useConsumeInventoryItemMutation } from '../../store/api/inventoryApi';

type Props = NativeStackScreenProps<InventoryStackParamList, 'ConsumeInventoryItem'>;

export default function ConsumeInventoryItemScreen({ route, navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const { item } = route.params;
  const unitCode = item.unit;
  const translatedUnit = t(`catalog.units.${unitCode}`, { defaultValue: unitCode });

  const [amount, setAmount] = useState('1');
  const [error, setError] = useState('');
  const [serverError, setServerError] = useState('');

  const [consumeItem, { isLoading }] = useConsumeInventoryItemMutation();

  function validate(): boolean {
    const val = parseFloat(amount);
    if (isNaN(val) || val <= 0) {
      setError(t('inventory.validation.amountInvalid'));
      return false;
    }
    if (val > item.quantity) {
      setError(t('inventory.validation.amountExceedsStock', { available: item.quantity, unit: translatedUnit }));
      return false;
    }
    setError('');
    return true;
  }

  async function handleSubmit() {
    if (!validate()) return;
    setServerError('');

    try {
      await consumeItem({ id: item.id, amount: parseFloat(amount) }).unwrap();
      navigation.goBack();
    } catch {
      setServerError(t('common.error'));
    }
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Card style={styles.card}>
        <Card.Content style={styles.cardContent}>
          <Text variant="titleMedium">{item.productName}</Text>
          {item.productBrand && (
            <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
              {item.productBrand}
            </Text>
          )}
          <Text variant="bodyMedium" style={styles.stockInfo}>
            {t('inventory.consume.currentStock', { quantity: item.quantity, unit: translatedUnit })}
          </Text>
        </Card.Content>
      </Card>

      <View style={styles.form}>
        <TextInput
          mode="outlined"
          label={t('inventory.consume.amount')}
          value={amount}
          onChangeText={setAmount}
          keyboardType="decimal-pad"
          error={!!error}
          right={<TextInput.Affix text={translatedUnit} />}
        />
        {error ? <HelperText type="error">{error}</HelperText> : null}

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
            {t('inventory.consume.submit')}
          </Button>
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 16, gap: 16 },
  card: {},
  cardContent: { gap: 4 },
  stockInfo: { marginTop: 8 },
  form: { gap: 8 },
  buttonRow: { flexDirection: 'row', gap: 12, marginTop: 8 },
  button: { flex: 1 },
});
