import React, { useState } from 'react';
import { StyleSheet, View } from 'react-native';
import { BottomNavigation, useTheme } from 'react-native-paper';
import type { Route } from 'react-native-paper/src/components/BottomNavigation/BottomNavigation';
import HomeScreen from '../screens/main/HomeScreen';
import CatalogNavigator from './CatalogNavigator';
import ProfileNavigator from './ProfileNavigator';

export default function MainTabNavigator() {
  const { colors } = useTheme();
  const [index, setIndex] = useState(0);

  const routes: Route[] = [
    { key: 'home', title: 'Inventory', focusedIcon: 'fridge', unfocusedIcon: 'fridge-outline' },
    { key: 'catalog', title: 'Catalog', focusedIcon: 'package-variant', unfocusedIcon: 'package-variant-closed' },
    { key: 'profile', title: 'Profile', focusedIcon: 'account-circle', unfocusedIcon: 'account-circle-outline' },
  ];

  const renderScene = ({ route }: { route: Route }) => {
    switch (route.key) {
      case 'home': return <HomeScreen />;
      case 'catalog': return (
        <View style={styles.fill}>
          <CatalogNavigator />
        </View>
      );
      case 'profile': return (
        <View style={styles.fill}>
          <ProfileNavigator />
        </View>
      );
      default: return null;
    }
  };

  return (
    <BottomNavigation
      navigationState={{ index, routes }}
      onIndexChange={setIndex}
      renderScene={renderScene}
      barStyle={{ backgroundColor: colors.surface }}
    />
  );
}

const styles = StyleSheet.create({
  fill: { flex: 1 },
});
