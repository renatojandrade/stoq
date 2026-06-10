import './src/i18n';
import React from 'react';
import { ActivityIndicator, View } from 'react-native';
import { Provider } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';
import { PaperProvider } from 'react-native-paper';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { StatusBar } from 'expo-status-bar';
import { store, persistor } from './src/store';
import { theme } from './src/theme';
import RootNavigator from './src/navigation/RootNavigator';

export default function App() {
  return (
    <Provider store={store}>
      <PersistGate loading={<View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}><ActivityIndicator size="large" /></View>} persistor={persistor}>
        <PaperProvider theme={theme}>
          <SafeAreaProvider>
            <StatusBar style="auto" />
            <RootNavigator />
          </SafeAreaProvider>
        </PaperProvider>
      </PersistGate>
    </Provider>
  );
}
