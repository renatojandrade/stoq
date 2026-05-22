import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import ShoppingListScreen from '../screens/shopping/ShoppingListScreen';
import AddShoppingItemScreen from '../screens/shopping/AddShoppingItemScreen';
import ReceiptScanListScreen from '../screens/shopping/ReceiptScanListScreen';
import ReceiptScanScreen from '../screens/shopping/ReceiptScanScreen';
import ReceiptConfirmScreen from '../screens/shopping/ReceiptConfirmScreen';
import type { ParsedReceiptItemDto } from '../store/api/receiptsApi';

export type ShoppingStackParamList = {
  ShoppingList: undefined;
  AddShoppingItem: undefined;
  ReceiptScanList: undefined;
  ReceiptScan: undefined;
  ReceiptConfirm:
    | { correctionId: string; items?: undefined; retailerName?: undefined }
    | { correctionId?: undefined; items: ParsedReceiptItemDto[]; retailerName: string | null };
};

const Stack = createNativeStackNavigator<ShoppingStackParamList>();

export default function ShoppingNavigator() {
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
        name="ShoppingList"
        component={ShoppingListScreen}
        options={{ title: t('shopping.list.title') }}
      />
      <Stack.Screen
        name="AddShoppingItem"
        component={AddShoppingItemScreen}
        options={{ title: t('shopping.add.title') }}
      />
      <Stack.Screen
        name="ReceiptScanList"
        component={ReceiptScanListScreen}
        options={{ title: t('shopping.scanList.title') }}
      />
      <Stack.Screen
        name="ReceiptScan"
        component={ReceiptScanScreen}
        options={{ title: t('shopping.receipt.scanTitle'), headerShown: false }}
      />
      <Stack.Screen
        name="ReceiptConfirm"
        component={ReceiptConfirmScreen}
        options={{ title: t('shopping.receipt.confirmTitle') }}
      />
    </Stack.Navigator>
  );
}
