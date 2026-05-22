import React from 'react';
import { Alert, StyleSheet, View } from 'react-native';
import { Avatar, Divider, List, Text, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { ProfileStackParamList } from '../../navigation/ProfileNavigator';
import type { RootState, AppDispatch } from '../../store';
import { clearCredentials, getStoredRefreshToken } from '../../store/authSlice';
import { useLogoutMutation, useRemoveDeviceTokenMutation } from '../../store/api/authApi';

type Props = NativeStackScreenProps<ProfileStackParamList, 'Profile'>;

export default function ProfileScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const dispatch = useDispatch<AppDispatch>();
  const user = useSelector((state: RootState) => state.auth.user);
  const [logout] = useLogoutMutation();
  const [removeDeviceToken] = useRemoveDeviceTokenMutation();

  async function handleLogout() {
    Alert.alert(
      t('profile.logoutTitle'),
      t('profile.logoutConfirm'),
      [
        { text: t('common.cancel'), style: 'cancel' },
        {
          text: t('profile.logout'),
          style: 'destructive',
          onPress: async () => {
            try {
              await removeDeviceToken();
              const token = await getStoredRefreshToken();
              if (token) await logout({ refreshToken: token });
            } catch {
              // Best-effort logout.
            }
            dispatch(clearCredentials());
          },
        },
      ],
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={[styles.header, { backgroundColor: colors.surface }]}>
        {user?.avatarUrl
          ? <Avatar.Image size={72} source={{ uri: user.avatarUrl }} />
          : <Avatar.Text size={72} label={(user?.displayName ?? 'U').charAt(0).toUpperCase()} />}
        <Text variant="titleLarge" style={styles.name}>{user?.displayName}</Text>
        <Text variant="bodyMedium" style={{ color: colors.onSurfaceVariant }}>{user?.email}</Text>
      </View>

      <Divider />

      <List.Item
        title={t('profile.householdSettings')}
        description={t('profile.householdSettingsDesc')}
        left={(props) => <List.Icon {...props} icon="home-account" />}
        right={(props) => <List.Icon {...props} icon="chevron-right" />}
        onPress={() => navigation.navigate('HouseholdSettings')}
      />

      <Divider />

      <List.Item
        title={t('profile.logout')}
        titleStyle={{ color: colors.error }}
        left={(props) => <List.Icon {...props} icon="logout" color={colors.error} />}
        onPress={handleLogout}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: { alignItems: 'center', padding: 32, gap: 8 },
  name: { marginTop: 4 },
});
