import React from 'react';
import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { useNavigation } from '@react-navigation/native';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import InventoryNavigator from './InventoryNavigator';
import CatalogNavigator from './CatalogNavigator';
import ProfileNavigator from './ProfileNavigator';
import ShoppingNavigator from './ShoppingNavigator';

const Tab = createBottomTabNavigator();

// Matches ui_kits/mobile/components.jsx TabBar label: 500 11px, numberOfLines=1.
// Using a render function is the only way to pass numberOfLines to the tab label.
function TabLabel({ label, color }: { label: string; color: string }) {
  return (
    <Text numberOfLines={1} style={[styles.label, { color }]}>
      {label}
    </Text>
  );
}

// Placeholder — ScanTab never actually mounts because tabBarButton intercepts all presses.
function ScanPlaceholder() {
  return <View />;
}

function ScanTabButton() {
  const { colors } = useTheme();
  const { t } = useTranslation();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const navigation = useNavigation<any>();

  function handlePress() {
    navigation.navigate('CatalogTab', { screen: 'BarcodeScanner' });
  }

  return (
    <TouchableOpacity
      onPress={handlePress}
      style={styles.scanButtonWrapper}
      accessibilityLabel={t('catalog.barcode.scanTitle')}
      accessibilityRole="button"
    >
      <View style={[styles.scanButtonCircle, { backgroundColor: colors.primary }]}>
        <MaterialCommunityIcons name="barcode-scan" size={26} color={colors.onPrimary} />
      </View>
    </TouchableOpacity>
  );
}

export default function MainTabNavigator() {
  const { colors } = useTheme();
  const { t } = useTranslation();
  const insets = useSafeAreaInsets();

  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.onSurfaceVariant,
        tabBarStyle: {
          backgroundColor: colors.surface,
          borderTopWidth: 1,
          borderTopColor: colors.outlineVariant,
          height: 62 + insets.bottom,
          paddingBottom: insets.bottom + 6,
          paddingHorizontal: 4,
        },
        // tabBarLabelStyle is applied when tabBarLabel is a string; with our render
        // function it has no effect, but kept as a fallback for any edge cases.
        tabBarLabelStyle: { fontSize: 11, fontWeight: '500' },
        tabBarItemStyle: { paddingTop: 6 },
      }}
    >
      <Tab.Screen
        name="InventoryTab"
        component={InventoryNavigator}
        options={{
          title: t('inventory.list.title'),
          tabBarLabel: ({ color }) => (
            <TabLabel label={t('inventory.list.title')} color={color} />
          ),
          tabBarIcon: ({ color, focused }) => (
            <MaterialCommunityIcons
              name={focused ? 'fridge' : 'fridge-outline'}
              size={24}
              color={color}
            />
          ),
        }}
      />
      <Tab.Screen
        name="ShoppingTab"
        component={ShoppingNavigator}
        options={{
          title: t('shopping.list.title'),
          tabBarLabel: ({ color }) => (
            <TabLabel label={t('shopping.list.tabLabel')} color={color} />
          ),
          tabBarIcon: ({ color, focused }) => (
            <MaterialCommunityIcons
              name={focused ? 'cart' : 'cart-outline'}
              size={24}
              color={color}
            />
          ),
        }}
      />
      <Tab.Screen
        name="ScanTab"
        component={ScanPlaceholder}
        options={{
          title: '',
          tabBarButton: () => <ScanTabButton />,
        }}
      />
      <Tab.Screen
        name="CatalogTab"
        component={CatalogNavigator}
        listeners={({ navigation }) => ({
          tabPress: () => {
            navigation.navigate('CatalogTab', { screen: 'ProductList' });
          },
        })}
        options={{
          title: t('catalog.list.title'),
          tabBarLabel: ({ color }) => (
            <TabLabel label={t('catalog.list.tabLabel')} color={color} />
          ),
          tabBarIcon: ({ color, focused }) => (
            <MaterialCommunityIcons
              name={focused ? 'package-variant' : 'package-variant-closed'}
              size={24}
              color={color}
            />
          ),
        }}
      />
      <Tab.Screen
        name="ProfileTab"
        component={ProfileNavigator}
        options={{
          title: t('profile.title'),
          tabBarLabel: ({ color }) => (
            <TabLabel label={t('profile.title')} color={color} />
          ),
          tabBarIcon: ({ color, focused }) => (
            <MaterialCommunityIcons
              name={focused ? 'account-circle' : 'account-circle-outline'}
              size={24}
              color={color}
            />
          ),
        }}
      />
    </Tab.Navigator>
  );
}

const styles = StyleSheet.create({
  label: {
    fontSize: 11,
    fontWeight: '500',
    includeFontPadding: false,
  } as const,
  scanButtonWrapper: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  scanButtonCircle: {
    width: 56,
    height: 56,
    borderRadius: 28,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: -14,
    elevation: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.25,
    shadowRadius: 4,
  },
});
