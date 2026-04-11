import React, { useState } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import {
  Button,
  HelperText,
  Menu,
  Text,
  TextInput,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CatalogStackParamList } from '../../navigation/CatalogNavigator';
import {
  useCreateProductMutation,
  useUpdateProductMutation,
  type ProductUnit,
} from '../../store/api/catalogApi';

type Props = NativeStackScreenProps<CatalogStackParamList, 'ProductForm'>;

const UNITS: ProductUnit[] = ['Units', 'Grams', 'Kilograms', 'Millilitres', 'Litres'];

export default function ProductFormScreen({ route, navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const existing = route.params?.product;

  const [name, setName] = useState(existing?.name ?? '');
  const [brand, setBrand] = useState(existing?.brand ?? '');
  const [category, setCategory] = useState(existing?.category ?? '');
  const [unit, setUnit] = useState<ProductUnit>(existing?.unit ?? 'Units');
  const [minQuantity, setMinQuantity] = useState(String(existing?.minQuantity ?? '0'));
  const [imageUrl, setImageUrl] = useState(existing?.imageUrl ?? '');
  const [barcode, setBarcode] = useState(existing?.barcode ?? '');

  const [unitMenuVisible, setUnitMenuVisible] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState('');

  const [createProduct, { isLoading: isCreating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();

  const isLoading = isCreating || isUpdating;

  function validate(): boolean {
    const next: Record<string, string> = {};

    if (!name.trim()) next['name'] = t('catalog.validation.nameRequired');
    if (!category.trim()) next['category'] = t('catalog.validation.categoryRequired');

    const qty = parseFloat(minQuantity);
    if (isNaN(qty) || qty < 0) next['minQuantity'] = t('catalog.validation.minQuantityInvalid');

    setErrors(next);
    return Object.keys(next).length === 0;
  }

  async function handleSubmit() {
    if (!validate()) return;
    setServerError('');

    const payload = {
      name: name.trim(),
      brand: brand.trim() || null,
      category: category.trim(),
      unit,
      minQuantity: parseFloat(minQuantity),
      imageUrl: imageUrl.trim() || null,
      barcode: barcode.trim() || null,
    };

    try {
      if (existing) {
        await updateProduct({ id: existing.id, ...payload }).unwrap();
      } else {
        await createProduct(payload).unwrap();
      }
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
        label={t('catalog.form.name')}
        value={name}
        onChangeText={setName}
        error={!!errors['name']}
      />
      {errors['name'] && <HelperText type="error">{errors['name']}</HelperText>}

      <TextInput
        mode="outlined"
        label={t('catalog.form.brand')}
        value={brand}
        onChangeText={setBrand}
        placeholder={t('catalog.form.brandPlaceholder')}
      />

      <TextInput
        mode="outlined"
        label={t('catalog.form.category')}
        value={category}
        onChangeText={setCategory}
        error={!!errors['category']}
      />
      {errors['category'] && <HelperText type="error">{errors['category']}</HelperText>}

      <Menu
        visible={unitMenuVisible}
        onDismiss={() => setUnitMenuVisible(false)}
        anchor={
          <TextInput
            mode="outlined"
            label={t('catalog.form.unit')}
            value={t(`catalog.units.${unit}`)}
            editable={false}
            right={<TextInput.Icon icon="chevron-down" onPress={() => setUnitMenuVisible(true)} />}
            onPressIn={() => setUnitMenuVisible(true)}
          />
        }
      >
        {UNITS.map((u) => (
          <Menu.Item
            key={u}
            title={t(`catalog.units.${u}`)}
            onPress={() => { setUnit(u); setUnitMenuVisible(false); }}
          />
        ))}
      </Menu>

      <TextInput
        mode="outlined"
        label={t('catalog.form.minQuantity')}
        value={minQuantity}
        onChangeText={setMinQuantity}
        keyboardType="decimal-pad"
        error={!!errors['minQuantity']}
      />
      {errors['minQuantity'] && <HelperText type="error">{errors['minQuantity']}</HelperText>}

      <TextInput
        mode="outlined"
        label={t('catalog.form.barcode')}
        value={barcode}
        onChangeText={setBarcode}
        placeholder={t('catalog.form.barcodePlaceholder')}
        keyboardType="number-pad"
      />

      <TextInput
        mode="outlined"
        label={t('catalog.form.imageUrl')}
        value={imageUrl}
        onChangeText={setImageUrl}
        placeholder="https://…"
        keyboardType="url"
        autoCapitalize="none"
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
          {existing ? t('catalog.form.update') : t('catalog.form.create')}
        </Button>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  content: { padding: 16, gap: 8 },
  buttonRow: { flexDirection: 'row', gap: 12, marginTop: 8 },
  button: { flex: 1 },
});
