import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Button, Text } from 'react-native-paper';
import { useDispatch, useSelector } from 'react-redux';
import { useTranslation } from 'react-i18next';
import type { RootState, AppDispatch } from '../../store';
import { clearCredentials } from '../../store/authSlice';
import { useLogoutMutation } from '../../store/api/authApi';

export default function HomeScreen() {
  const { t } = useTranslation();
  const dispatch = useDispatch<AppDispatch>();
  const user = useSelector((state: RootState) => state.auth.user);
  const [logout] = useLogoutMutation();

  async function handleLogout() {
    try {
      const { getStoredRefreshToken } = await import('../../store/authSlice');
      const token = await getStoredRefreshToken();
      if (token) await logout({ refreshToken: token });
    } catch {
      // Best-effort logout — clear local state regardless.
    }
    dispatch(clearCredentials());
  }

  return (
    <View style={styles.container}>
      <Text variant="headlineMedium">
        {t('common.loading')}
      </Text>
      <Text variant="bodyMedium" style={styles.sub}>
        Signed in as {user?.displayName}
      </Text>
      <Button mode="outlined" onPress={handleLogout} style={styles.button}>
        Sign out
      </Button>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center', alignItems: 'center' },
  sub: { marginTop: 8, marginBottom: 32, opacity: 0.6 },
  button: {},
});
