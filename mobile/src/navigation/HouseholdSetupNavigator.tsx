import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import HouseholdSetupScreen from '../screens/household/HouseholdSetupScreen';
import CreateHouseholdScreen from '../screens/household/CreateHouseholdScreen';
import JoinHouseholdScreen from '../screens/household/JoinHouseholdScreen';

export type HouseholdStackParamList = {
  HouseholdSetup: undefined;
  CreateHousehold: undefined;
  JoinHousehold: undefined;
};

const Stack = createNativeStackNavigator<HouseholdStackParamList>();

export default function HouseholdSetupNavigator() {
  return (
    <Stack.Navigator
      initialRouteName="HouseholdSetup"
      screenOptions={{ headerShown: false }}
    >
      <Stack.Screen name="HouseholdSetup" component={HouseholdSetupScreen} />
      <Stack.Screen name="CreateHousehold" component={CreateHouseholdScreen} />
      <Stack.Screen name="JoinHousehold" component={JoinHouseholdScreen} />
    </Stack.Navigator>
  );
}
