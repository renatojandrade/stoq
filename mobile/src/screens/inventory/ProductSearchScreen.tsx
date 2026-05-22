import React, { useCallback, useEffect, useRef, useState } from 'react';
import { FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Divider,
  List,
  Searchbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { InventoryStackParamList } from '../../navigation/InventoryNavigator';
import { useListProductsQuery, type ProductDto } from '../../store/api/catalogApi';

type Props = NativeStackScreenProps<InventoryStackParamList, 'ProductSearch'>;

const PAGE_SIZE = 20;

export default function ProductSearchScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [allProducts, setAllProducts] = useState<ProductDto[]>([]);
  const searchRef = useRef(search);
  searchRef.current = search;

  const activeSearch = search.length >= 4 ? search : undefined;

  const { data, isFetching, isError } = useListProductsQuery(
    { name: activeSearch, page, pageSize: PAGE_SIZE },
    { skip: !activeSearch, refetchOnFocus: false },
  );

  useEffect(() => {
    if (data) {
      if (page === 1) {
        setAllProducts(data);
      } else {
        setAllProducts((prev) => [...prev, ...data]);
      }
    }
  }, [data, page]);

  function handleSearchChange(text: string) {
    setSearch(text);
    setPage(1);
    if (text.length < 4) {
      setAllProducts([]);
    }
  }

  const handleEndReached = useCallback(() => {
    if (!isFetching && data && data.length === PAGE_SIZE) {
      setPage((prev) => prev + 1);
    }
  }, [isFetching, data]);

  function handleSelect(product: ProductDto) {
    navigation.navigate('AddInventoryItem', {
      selectedProductId: product.id,
      selectedProductName: product.name,
      selectedProductUnit: product.unitCode,
    });
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Searchbar
        placeholder={t('inventory.productSearch.placeholder')}
        value={search}
        onChangeText={handleSearchChange}
        autoFocus
        style={styles.searchbar}
      />

      <FlatList
        data={allProducts}
        keyExtractor={(item) => item.id}
        keyboardShouldPersistTaps="handled"
        ItemSeparatorComponent={() => <Divider />}
        renderItem={({ item }) => (
          <List.Item
            title={item.name}
            description={[item.brandName, item.categoryNameEn].filter(Boolean).join(' · ')}
            onPress={() => handleSelect(item)}
            right={(props) =>
              item.isReference ? null : (
                <List.Icon {...props} icon="home-outline" />
              )
            }
          />
        )}
        onEndReached={handleEndReached}
        onEndReachedThreshold={0.3}
        ListEmptyComponent={
          isFetching ? null : (
            <View style={styles.empty}>
              <Text variant="bodyMedium" style={{ color: isError ? colors.error : colors.onSurfaceVariant }}>
                {isError
                  ? t('common.loadError')
                  : activeSearch
                    ? t('inventory.productSearch.noResults')
                    : t('inventory.productSearch.startTyping')}
              </Text>
            </View>
          )
        }
        ListFooterComponent={
          isFetching ? (
            <View style={styles.footer}>
              <ActivityIndicator />
            </View>
          ) : null
        }
        contentContainerStyle={allProducts.length === 0 ? styles.emptyContainer : undefined}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  searchbar: { margin: 12 },
  empty: { alignItems: 'center', paddingTop: 40 },
  emptyContainer: { flex: 1 },
  footer: { paddingVertical: 16, alignItems: 'center' },
});
