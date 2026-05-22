import React, { useLayoutEffect, useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  FAB,
  IconButton,
  List,
  Menu,
  Searchbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CatalogStackParamList } from '../../navigation/CatalogNavigator';
import { useListProductsQuery, useDeleteProductMutation } from '../../store/api/catalogApi';
import type { ProductDto, ListProductsParams } from '../../store/api/catalogApi';

type Props = NativeStackScreenProps<CatalogStackParamList, 'ProductList'>;

type FilterType = 'all' | 'general' | 'household';

export default function ProductListScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<FilterType>('all');
  const [fabOpen, setFabOpen] = useState(false);
  const [filterMenuVisible, setFilterMenuVisible] = useState(false);

  const activeSearch = search.length >= 4 ? search : '';

  const requiresSearch = filter !== 'household';
  const shouldSkip = requiresSearch && !activeSearch;

  const queryParams: ListProductsParams = {};
  if (activeSearch) queryParams.name = activeSearch;
  if (filter === 'general') queryParams.isReference = true;
  if (filter === 'household') queryParams.isReference = false;

  const { data: products, isLoading, isFetching, isError, refetch } = useListProductsQuery(
    Object.keys(queryParams).length > 0 ? queryParams : undefined,
    { skip: shouldSkip, refetchOnFocus: false },
  );

  const [deleteProduct] = useDeleteProductMutation();

  useLayoutEffect(() => {
    navigation.setOptions({
      headerRight: () => (
        <Menu
          visible={filterMenuVisible}
          onDismiss={() => setFilterMenuVisible(false)}
          anchor={
            <IconButton
              icon="filter-variant"
              size={22}
              onPress={() => setFilterMenuVisible(true)}
              accessibilityLabel={t('catalog.list.filter')}
            />
          }
        >
          <Menu.Item
            leadingIcon={filter === 'all' ? 'check' : undefined}
            onPress={() => { setFilter('all'); setFilterMenuVisible(false); }}
            title={t('catalog.list.filterAll')}
          />
          <Menu.Item
            leadingIcon={filter === 'general' ? 'check' : undefined}
            onPress={() => { setFilter('general'); setFilterMenuVisible(false); }}
            title={t('catalog.list.filterGeneral')}
          />
          <Menu.Item
            leadingIcon={filter === 'household' ? 'check' : undefined}
            onPress={() => { setFilter('household'); setFilterMenuVisible(false); }}
            title={t('catalog.list.filterHousehold')}
          />
        </Menu>
      ),
    });
  }, [navigation, filterMenuVisible, filter, t]);

  function handleEdit(product: ProductDto) {
    navigation.navigate('ProductForm', { product });
  }

  async function handleDelete(id: string) {
    await deleteProduct(id);
  }

  const showSearchPrompt = shouldSkip;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Searchbar
        placeholder={t('catalog.list.searchPlaceholder')}
        value={search}
        onChangeText={setSearch}
        style={styles.searchbar}
      />

      {showSearchPrompt ? (
        <View style={styles.center}>
          <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant, textAlign: 'center' }}>
            {t('catalog.list.searchPrompt')}
          </Text>
        </View>
      ) : isLoading || isFetching ? (
        <View style={styles.center}>
          <ActivityIndicator />
        </View>
      ) : isError ? (
        <View style={styles.center}>
          <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant }}>
            {t('common.error')}
          </Text>
          <IconButton icon="refresh" onPress={refetch} />
        </View>
      ) : products?.length === 0 ? (
        <View style={styles.center}>
          <Text variant="bodyLarge" style={{ color: colors.onSurfaceVariant, textAlign: 'center' }}>
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
              description={[item.brandName, item.categoryNameEn].filter(Boolean).join(' · ')}
              left={(props) => <List.Icon {...props} icon={item.isReference ? 'package-variant' : 'home-outline'} />}
              right={item.isReference ? undefined : () => (
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
        open={fabOpen}
        visible
        icon={fabOpen ? 'close' : 'plus'}
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
        onStateChange={({ open }) => setFabOpen(open)}
        accessibilityLabel={t('catalog.list.addProduct')}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 8 },
  searchbar: { margin: 16, marginBottom: 8 },
  list: { paddingBottom: 96 },
  actions: { flexDirection: 'row', alignItems: 'center' },
  fabGroup: { position: 'absolute', right: 0, bottom: 0 },
});
