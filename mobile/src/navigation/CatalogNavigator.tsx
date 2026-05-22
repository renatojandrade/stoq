import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import ProductListScreen from '../screens/catalog/ProductListScreen';
import ProductDetailScreen from '../screens/catalog/ProductDetailScreen';
import ProductFormScreen from '../screens/catalog/ProductFormScreen';
import BarcodeScanScreen from '../screens/catalog/BarcodeScanScreen';
import type { ProductDto } from '../store/api/catalogApi';

export interface ProductPrefill {
  barcode?: string;
  name?: string;
  brandName?: string | null;
  imageUrl?: string | null;
}

export type CatalogStackParamList = {
  ProductList: undefined;
  ProductDetail: { productId: string };
  ProductForm: { product?: ProductDto; prefill?: ProductPrefill };
  BarcodeScanner: undefined;
};

const Stack = createNativeStackNavigator<CatalogStackParamList>();

export default function CatalogNavigator() {
  const { colors } = useTheme();
  const { t } = useTranslation();

  return (
    <Stack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: colors.surface },
        headerTintColor: colors.onSurface,
        headerShadowVisible: false,
      }}
    >
      <Stack.Screen
        name="ProductList"
        component={ProductListScreen}
        options={{ title: t('catalog.list.title') }}
      />
      <Stack.Screen
        name="ProductDetail"
        component={ProductDetailScreen}
        options={{ title: t('catalog.detail.title') }}
      />
      <Stack.Screen
        name="ProductForm"
        component={ProductFormScreen}
        options={({ route }) => ({
          title: route.params?.product ? t('catalog.form.editTitle') : t('catalog.form.createTitle'),
        })}
      />
      <Stack.Screen
        name="BarcodeScanner"
        component={BarcodeScanScreen}
        options={{ title: t('catalog.barcode.scanTitle'), headerShown: false }}
      />
    </Stack.Navigator>
  );
}
