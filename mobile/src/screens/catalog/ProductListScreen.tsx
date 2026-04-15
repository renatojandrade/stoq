import React, { useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  FAB,
  IconButton,
  List,
  Searchbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CatalogStackParamList } from '../../navigation/CatalogNavigator';
import { useListProductsQuery, useDeleteProductMutation } from '../../store/api/catalogApi';
import type { ProductDto } from '../../store/api/catalogApi';

type Props = NativeStackScreenProps<CatalogStackParamList, 'ProductList'>;

export default function ProductListScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [search, setSearch] = useState('');

  const { data: products, isLoading, isError, refetch } = useListProductsQuery(
    search ? { name: search } : undefined,
    { refetchOnFocus: true },
  );

  const [deleteProduct] = useDeleteProductMutation();

  function handleEdit(product: ProductDto) {
    navigation.navigate('ProductForm', { product });
  }

  async function handleDelete(id: string) {
    await deleteProduct(id);
  }

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
        <Text>{t('common.retry')}</Text>
        <IconButton icon="refresh" onPress={refetch} />
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Searchbar
        placeholder={t('catalog.list.searchPlaceholder')}
        value={search}
        onChangeText={setSearch}
        style={styles.searchbar}
      />

      {products?.length === 0 ? (
        <View style={styles.center}>
          <Text variant="bodyLarge" style={{ color: colors.onSurfaceVariant }}>
            {t('catalog.list.empty')}
          </Text>
        </View>
      ) : (
        <FlatList
          data={products}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <List.Item
              title={item.name}
              description={[item.brand, item.category].filter(Boolean).join(' · ')}
              left={(props) => <List.Icon {...props} icon="package-variant" />}
              right={() => (
                <View style={styles.actions}>
                  <IconButton
                    icon="pencil-outline"
                    size={20}
                    onPress={() => handleEdit(item)}
                    accessibilityLabel={t('catalog.list.edit')}
                  />
                  <IconButton
                    icon="trash-can-outline"
                    size={20}
                    iconColor={colors.error}
                    onPress={() => handleDelete(item.id)}
                    accessibilityLabel={t('catalog.list.delete')}
                  />
                </View>
              )}
              onPress={() => navigation.navigate('ProductDetail', { productId: item.id })}
            />
          )}
          contentContainerStyle={styles.list}
        />
      )}

      <FAB.Group
        open={false}
        visible
        icon="plus"
        style={styles.fabGroup}
        actions={[
          {
            icon: 'barcode-scan',
            label: t('catalog.list.scanBarcode'),
            onPress: () => navigation.navigate('BarcodeScanner'),
            accessibilityLabel: t('catalog.list.scanBarcode'),
          },
          {
            icon: 'pencil-plus-outline',
            label: t('catalog.list.addProduct'),
            onPress: () => navigation.navigate('ProductForm', {}),
            accessibilityLabel: t('catalog.list.addProduct'),
          },
        ]}
        onStateChange={() => {}}
        accessibilityLabel={t('catalog.list.addProduct')}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 8 },
  searchbar: { margin: 16 },
  list: { paddingBottom: 96 },
  actions: { flexDirection: 'row', alignItems: 'center' },
  fabGroup: { position: 'absolute', right: 0, bottom: 0 },
});
