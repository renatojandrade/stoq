import React from 'react';
import { StyleSheet, TouchableOpacity, View } from 'react-native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { useNavigation } from '@react-navigation/native';
import { useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import InventoryNavigator from './InventoryNavigator';
import CatalogNavigator from './CatalogNavigator';
import ProfileNavigator from './ProfileNavigator';

const Tab = createBottomTabNavigator();

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

  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.onSurfaceVariant,
        tabBarStyle: { backgroundColor: colors.surface, borderTopColor: colors.surfaceVariant },
      }}
    >
      <Tab.Screen
        name="InventoryTab"
        component={InventoryNavigator}
        options={{
          title: t('inventory.list.title'),
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
        options={{
          title: t('catalog.list.title'),
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
