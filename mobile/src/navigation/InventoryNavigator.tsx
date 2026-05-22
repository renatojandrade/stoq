import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import InventoryDashboardScreen from '../screens/inventory/InventoryDashboardScreen';
import AddInventoryItemScreen from '../screens/inventory/AddInventoryItemScreen';
import ConsumeInventoryItemScreen from '../screens/inventory/ConsumeInventoryItemScreen';
import ExpiryAlertsScreen from '../screens/inventory/ExpiryAlertsScreen';
import ProductInventoryDetailScreen from '../screens/inventory/ProductInventoryDetailScreen';
import ProductSearchScreen from '../screens/inventory/ProductSearchScreen';
import type { InventoryItemDto } from '../store/api/inventoryApi';

export type InventoryStackParamList = {
  InventoryDashboard: undefined;
  ExpiryAlerts: undefined;
  ProductInventoryDetail: {
    productId: string;
    productName: string;
    productBrand: string | null;
    productImageUrl: string | null;
    unit: string;
    minQuantity: number;
  };
  AddInventoryItem: {
    preselectedProductId?: string;
    preselectedProductName?: string;
    preselectedUnit?: string;
    selectedProductId?: string;
    selectedProductName?: string;
    selectedProductUnit?: string;
    skipToRoot?: boolean;
  } | undefined;
  ProductSearch: undefined;
  ConsumeInventoryItem: { item: InventoryItemDto };
};

const Stack = createNativeStackNavigator<InventoryStackParamList>();

export default function InventoryNavigator() {
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
        name="InventoryDashboard"
        component={InventoryDashboardScreen}
        options={{ title: t('inventory.list.title') }}
      />
      <Stack.Screen
        name="ExpiryAlerts"
        component={ExpiryAlertsScreen}
        options={{ title: t('inventory.expiry.title') }}
      />
      <Stack.Screen
        name="ProductInventoryDetail"
        component={ProductInventoryDetailScreen}
        options={({ route }) => ({ title: route.params.productName })}
      />
      <Stack.Screen
        name="AddInventoryItem"
        component={AddInventoryItemScreen}
        options={{ title: t('inventory.add.title') }}
      />
      <Stack.Screen
        name="ProductSearch"
        component={ProductSearchScreen}
        options={{ title: t('inventory.productSearch.title') }}
      />
      <Stack.Screen
        name="ConsumeInventoryItem"
        component={ConsumeInventoryItemScreen}
        options={{ title: t('inventory.consume.title') }}
      />
    </Stack.Navigator>
  );
}
