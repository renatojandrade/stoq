import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { useTheme } from 'react-native-paper';
import ProfileScreen from '../screens/profile/ProfileScreen';
import HouseholdSettingsScreen from '../screens/household/HouseholdSettingsScreen';
import ManageMembersScreen from '../screens/household/ManageMembersScreen';

export type ProfileStackParamList = {
  Profile: undefined;
  HouseholdSettings: undefined;
  ManageMembers: undefined;
};

const Stack = createNativeStackNavigator<ProfileStackParamList>();

export default function ProfileNavigator() {
  const { colors } = useTheme();

  return (
    <Stack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: colors.surface },
        headerTintColor: colors.onSurface,
        headerShadowVisible: false,
      }}
    >
      <Stack.Screen
        name="Profile"
        component={ProfileScreen}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name="HouseholdSettings"
        component={HouseholdSettingsScreen}
        options={{ title: 'Household' }}
      />
      <Stack.Screen
        name="ManageMembers"
        component={ManageMembersScreen}
        options={{ title: 'Members' }}
      />
    </Stack.Navigator>
  );
}
