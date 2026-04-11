import React, { useState } from 'react';
import { Alert, Share, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Card,
  Divider,
  List,
  Snackbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useSelector } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { ProfileStackParamList } from '../../navigation/ProfileNavigator';
import {
  useGetMyHouseholdQuery,
  useRegenerateInviteCodeMutation,
} from '../../store/api/authApi';
import type { RootState } from '../../store';

type Props = NativeStackScreenProps<ProfileStackParamList, 'HouseholdSettings'>;

export default function HouseholdSettingsScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const currentUserId = useSelector((state: RootState) => state.auth.user?.id);

  const { data: household, isLoading, isError, refetch } = useGetMyHouseholdQuery();
  const [regenerate, { isLoading: isRegenerating }] = useRegenerateInviteCodeMutation();

  const [snackbar, setSnackbar] = useState<string | null>(null);

  const isAdmin = household?.members.some(
    (m) => m.userId === currentUserId && m.role === 'HouseholdAdmin',
  ) ?? false;

  async function handleRegenerate() {
    if (!household) return;
    Alert.alert(
      t('household.settings.regenerateTitle'),
      t('household.settings.regenerateConfirm'),
      [
        { text: t('common.cancel'), style: 'cancel' },
        {
          text: t('household.settings.regenerate'),
          style: 'destructive',
          onPress: async () => {
            try {
              await regenerate({ householdId: household.id }).unwrap();
              setSnackbar(t('household.settings.regenerateSuccess'));
            } catch {
              setSnackbar(t('auth.errors.unknown'));
            }
          },
        },
      ],
    );
  }

  async function handleShare() {
    if (!household) return;
    await Share.share({
      message: t('household.create.shareMessage', {
        code: household.inviteCode,
        name: household.name,
      }),
    });
  }

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  if (isError || !household) {
    return (
      <View style={styles.center}>
        <Text variant="bodyMedium" style={{ color: colors.error }}>
          {t('auth.errors.unknown')}
        </Text>
        <Button onPress={refetch} style={styles.retryButton}>{t('common.retry')}</Button>
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <Text variant="titleMedium" style={[styles.sectionLabel, { color: colors.onSurfaceVariant }]}>
        {t('household.settings.inviteCodeSection')}
      </Text>

      <Card style={styles.codeCard} mode="outlined">
        <Card.Content style={styles.codeCardContent}>
          <Text variant="displaySmall" style={[styles.codeText, { color: colors.primary }]}>
            {household.inviteCode}
          </Text>
          <Text variant="bodySmall" style={{ color: household.isInviteCodeExpired ? colors.error : colors.onSurfaceVariant }}>
            {household.isInviteCodeExpired
              ? t('household.settings.codeExpired')
              : t('household.settings.codeExpires', {
                  date: new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' })
                    .format(new Date(household.inviteCodeExpiresAt)),
                })}
          </Text>
        </Card.Content>
      </Card>

      <View style={styles.row}>
        <Button
          mode="outlined"
          icon="share-variant"
          onPress={handleShare}
          style={styles.flex}
        >
          {t('household.create.shareCode')}
        </Button>

        {isAdmin && (
          <Button
            mode="outlined"
            icon="refresh"
            onPress={handleRegenerate}
            loading={isRegenerating}
            disabled={isRegenerating}
            style={[styles.flex, styles.marginStart]}
          >
            {t('household.settings.regenerate')}
          </Button>
        )}
      </View>

      <Divider style={styles.divider} />

      {isAdmin && (
        <List.Item
          title={t('household.settings.manageMembers')}
          description={t('household.settings.manageMembersDesc', { count: household.members.length })}
          left={(props) => <List.Icon {...props} icon="account-group" />}
          right={(props) => <List.Icon {...props} icon="chevron-right" />}
          onPress={() => navigation.navigate('ManageMembers')}
        />
      )}

      <Snackbar
        visible={snackbar !== null}
        onDismiss={() => setSnackbar(null)}
        duration={3000}
      >
        {snackbar}
      </Snackbar>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 16 },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  sectionLabel: { marginBottom: 8, marginTop: 8 },
  codeCard: { marginBottom: 16 },
  codeCardContent: { alignItems: 'center', paddingVertical: 16, gap: 8 },
  codeText: { letterSpacing: 8, fontWeight: 'bold' },
  row: { flexDirection: 'row', marginBottom: 16 },
  flex: { flex: 1 },
  marginStart: { marginStart: 8 },
  divider: { marginVertical: 8 },
  retryButton: { marginTop: 12 },
});
