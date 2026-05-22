import React, { useState } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
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
  useListCategoriesQuery,
  useListUnitsQuery,
  type CategoryDto,
  type UnitDto,
} from '../../store/api/catalogApi';

type Props = NativeStackScreenProps<CatalogStackParamList, 'ProductForm'>;

export default function ProductFormScreen({ route, navigation }: Props) {
  const { t, i18n } = useTranslation();
  const { colors } = useTheme();
  const existing = route.params?.product;
  const prefill = route.params?.prefill;

  const { data: categories = [], isLoading: loadingCategories } = useListCategoriesQuery();
  const { data: units = [], isLoading: loadingUnits } = useListUnitsQuery();

  const [name, setName] = useState(existing?.name ?? prefill?.name ?? '');
  const [brandName, setBrandName] = useState(existing?.brandName ?? prefill?.brandName ?? '');
  const [categoryId, setCategoryId] = useState(existing?.categoryId ?? '');
  const [unitId, setUnitId] = useState(existing?.unitId ?? '');
  const [minQuantity, setMinQuantity] = useState(String(existing?.minQuantity ?? '0'));
  const [imageUrl, setImageUrl] = useState(existing?.imageUrl ?? prefill?.imageUrl ?? '');
  const [barcode, setBarcode] = useState(existing?.barcode ?? prefill?.barcode ?? '');

  const [categoryMenuVisible, setCategoryMenuVisible] = useState(false);
  const [unitMenuVisible, setUnitMenuVisible] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState('');

  const [createProduct, { isLoading: isCreating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();

  const isLoading = isCreating || isUpdating;
  const isLoadingPickerData = loadingCategories || loadingUnits;

  const selectedCategory = categories.find((c) => c.id === categoryId);
  const selectedUnit = units.find((u) => u.id === unitId);

  function categoryLabel(c: CategoryDto): string {
    return i18n.language.startsWith('pt') ? c.namePt : c.nameEn;
  }

  function unitLabel(u: UnitDto): string {
    return i18n.language.startsWith('pt') ? u.namePt : u.nameEn;
  }

  function validate(): boolean {
    const next: Record<string, string> = {};

    if (!name.trim()) next['name'] = t('catalog.validation.nameRequired');
    if (!categoryId) next['category'] = t('catalog.validation.categoryRequired');
    if (!unitId) next['unit'] = t('catalog.validation.unitRequired');

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
      categoryId,
      brandName: brandName.trim() || null,
      unitId,
      minQuantity: parseFloat(minQuantity),
      imageUrl: imageUrl.trim() || null,
      barcode: barcode.trim() || null,
    };

    try {
      if (existing) {
        await updateProduct({ id: existing.id, ...payload }).unwrap();
        navigation.navigate('ProductList');
      } else {
        const created = await createProduct(payload).unwrap();
        navigation.replace('ProductDetail', { productId: created.id });
      }
    } catch {
      setServerError(t('common.error'));
    }
  }

  if (isLoadingPickerData) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
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
        value={brandName}
        onChangeText={setBrandName}
        placeholder={t('catalog.form.brandPlaceholder')}
      />

      <Menu
        visible={categoryMenuVisible}
        onDismiss={() => setCategoryMenuVisible(false)}
        anchor={
          <TextInput
            mode="outlined"
            label={t('catalog.form.category')}
            value={selectedCategory ? categoryLabel(selectedCategory) : ''}
            editable={false}
            error={!!errors['category']}
            right={<TextInput.Icon icon="chevron-down" onPress={() => setCategoryMenuVisible(true)} />}
            onPressIn={() => setCategoryMenuVisible(true)}
          />
        }
      >
        {categories.map((c) => (
          <Menu.Item
            key={c.id}
            title={categoryLabel(c)}
            onPress={() => { setCategoryId(c.id); setCategoryMenuVisible(false); }}
          />
        ))}
      </Menu>
      {errors['category'] && <HelperText type="error">{errors['category']}</HelperText>}

      <Menu
        visible={unitMenuVisible}
        onDismiss={() => setUnitMenuVisible(false)}
        anchor={
          <TextInput
            mode="outlined"
            label={t('catalog.form.unit')}
            value={selectedUnit ? unitLabel(selectedUnit) : ''}
            editable={false}
            error={!!errors['unit']}
            right={<TextInput.Icon icon="chevron-down" onPress={() => setUnitMenuVisible(true)} />}
            onPressIn={() => setUnitMenuVisible(true)}
          />
        }
      >
        {units.map((u) => (
          <Menu.Item
            key={u.id}
            title={unitLabel(u)}
            onPress={() => { setUnitId(u.id); setUnitMenuVisible(false); }}
          />
        ))}
      </Menu>
      {errors['unit'] && <HelperText type="error">{errors['unit']}</HelperText>}

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
        right={
          !existing ? (
            <TextInput.Icon
              icon="barcode-scan"
              onPress={() => navigation.navigate('BarcodeScanner')}
              accessibilityLabel={t('catalog.barcode.scanTitle')}
            />
          ) : undefined
        }
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
        <Button mode="outlined" onPress={() => navigation.navigate('ProductList')} style={styles.button}>
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
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  content: { padding: 16, gap: 8 },
  buttonRow: { flexDirection: 'row', gap: 12, marginTop: 8 },
  button: { flex: 1 },
});
