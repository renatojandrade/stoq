import React, { useCallback, useEffect, useState } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import { createNavigationContainerRef, NavigationContainer } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';
import type { RootState, AppDispatch } from '../store';
import { setCredentials, getStoredRefreshToken } from '../store/authSlice';
import { authApi } from '../store/api/authApi';
import { receiptsApi } from '../store/api/receiptsApi';
import { usePushNotifications } from '../hooks/usePushNotifications';
import { useSignalR } from '../hooks/useSignalR';
import AuthNavigator from './AuthNavigator';
import HouseholdSetupNavigator from './HouseholdSetupNavigator';
import MainTabNavigator from './MainTabNavigator';

const navigationRef = createNavigationContainerRef();

export default function RootNavigator() {
  const dispatch = useDispatch<AppDispatch>();
  const { t } = useTranslation();
  const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated);
  const householdId = useSelector((state: RootState) => state.auth.householdId);
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);
  const [isBootstrapping, setIsBootstrapping] = useState(true);

  const connection = useSignalR();

  // Global handlers for async NF-e scan results — survive screen navigation.
  // Invalidate the corrections list cache so ReceiptScanList updates automatically.
  useEffect(() => {
    if (!connection) return;

    const handleCorrectionReady = (_correctionId: string) => {
      dispatch(receiptsApi.util.invalidateTags(['ParseCorrection']));
    };

    const handleCorrectionFailed = (_correctionId: string, _reason: string) => {
      dispatch(receiptsApi.util.invalidateTags(['ParseCorrection']));
    };

    connection.on('ReceiptCorrectionReady', handleCorrectionReady);
    connection.on('ReceiptCorrectionFailed', handleCorrectionFailed);

    return () => {
      connection.off('ReceiptCorrectionReady', handleCorrectionReady);
      connection.off('ReceiptCorrectionFailed', handleCorrectionFailed);
    };
  }, [connection, dispatch]);

  const handleNotificationTap = useCallback((data: Record<string, string>) => {
    if (!navigationRef.isReady()) {
      return;
    }

    const screen = data['screen'];

    if (screen === 'ExpiryAlerts') {
      navigationRef.navigate('InventoryTab' as never);
    } else if (screen === 'ShoppingList') {
      navigationRef.navigate('ShoppingTab' as never);
    } else if (screen === 'ScanList') {
      navigationRef.navigate('ShoppingTab' as never, { screen: 'ScanList' } as never);
    }
  }, []);

  usePushNotifications(isAuthenticated ? accessToken : null, handleNotificationTap);

  // On cold start: attempt to restore session from the persisted refresh token.
  useEffect(() => {
    let cancelled = false;

    async function restoreSession() {
      try {
        const refreshToken = await getStoredRefreshToken();
        if (!refreshToken || cancelled) { return; }

        const result = await dispatch(
          authApi.endpoints.refresh.initiate({
            expiredAccessToken: '',
            refreshToken,
          }),
        ).unwrap();

        if (!cancelled) {
          dispatch(setCredentials(result));
        }
      } catch {
        // Refresh failed — user lands on auth screens.
      } finally {
        if (!cancelled) setIsBootstrapping(false);
      }
    }

    void restoreSession();
    return () => { cancelled = true; };
  }, [dispatch]);

  if (isBootstrapping) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  return (
    <NavigationContainer ref={navigationRef}>
      {!isAuthenticated
        ? <AuthNavigator />
        : householdId
          ? <MainTabNavigator />
          : <HouseholdSetupNavigator />}
    </NavigationContainer>
  );
}
