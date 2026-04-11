import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, Share, StyleSheet, View } from 'react-native';
import { Button, Card, HelperText, Text, TextInput, useTheme } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useDispatch } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { HouseholdStackParamList } from '../../navigation/HouseholdSetupNavigator';
import { useCreateHouseholdMutation, type HouseholdDto } from '../../store/api/authApi';
import { setHousehold } from '../../store/authSlice';

type Props = NativeStackScreenProps<HouseholdStackParamList, 'CreateHousehold'>;

export default function CreateHouseholdScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const dispatch = useDispatch();
  const [createHousehold, { isLoading }] = useCreateHouseholdMutation();

  const [name, setName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [created, setCreated] = useState<HouseholdDto | null>(null);

  async function handleCreate() {
    setError(null);
    if (!name.trim()) { setError(t('household.create.nameRequired')); return; }

    try {
      const household = await createHousehold({ name: name.trim() }).unwrap();
      setCreated(household);
    } catch (err: unknown) {
      const message = (err as { data?: { detail?: string } })?.data?.detail;
      setError(message ?? t('auth.errors.unknown'));
    }
  }

  async function handleShare() {
    if (!created) return;
    await Share.share({
      message: t('household.create.shareMessage', { code: created.inviteCode, name: created.name }),
    });
  }

  function handleContinue() {
    if (created) {
      dispatch(setHousehold(created.id));
    }
  }

  if (created) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <Text variant="headlineMedium" style={styles.title}>
          {t('household.create.successTitle')}
        </Text>
        <Text variant="bodyMedium" style={[styles.subtitle, { color: colors.onSurfaceVariant }]}>
          {t('household.create.successSubtitle')}
        </Text>

        <Card style={styles.codeCard} mode="outlined">
          <Card.Content style={styles.codeCardContent}>
            <Text variant="labelMedium" style={{ color: colors.onSurfaceVariant }}>
              {t('household.create.inviteCodeLabel')}
            </Text>
            <Text variant="displaySmall" style={[styles.codeText, { color: colors.primary }]}>
              {created.inviteCode}
            </Text>
            <Text variant="bodySmall" style={{ color: colors.onSurfaceVariant }}>
              {t('household.create.inviteCodeExpiry')}
            </Text>
          </Card.Content>
        </Card>

        <Button
          mode="outlined"
          icon="share-variant"
          onPress={handleShare}
          style={styles.button}
        >
          {t('household.create.shareCode')}
        </Button>

        <Button
          mode="contained"
          onPress={handleContinue}
          style={styles.button}
        >
          {t('household.create.continue')}
        </Button>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={[styles.container, { backgroundColor: colors.background }]}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <Text variant="headlineMedium" style={styles.title}>{t('household.create.title')}</Text>

      <TextInput
        label={t('household.create.name')}
        value={name}
        onChangeText={setName}
        placeholder={t('household.create.namePlaceholder')}
        style={styles.input}
        mode="outlined"
        autoFocus
      />

      {error && <HelperText type="error">{error}</HelperText>}

      <Button
        mode="contained"
        onPress={handleCreate}
        loading={isLoading}
        disabled={isLoading}
        style={styles.button}
      >
        {t('household.create.submit')}
      </Button>

      <Button mode="text" onPress={() => navigation.goBack()} style={styles.backButton}>
        {t('common.cancel')}
      </Button>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center' },
  title: { marginBottom: 8 },
  subtitle: { marginBottom: 24 },
  input: { marginBottom: 12 },
  button: { marginTop: 8 },
  backButton: { marginTop: 4 },
  codeCard: { marginVertical: 24 },
  codeCardContent: { alignItems: 'center', paddingVertical: 16, gap: 8 },
  codeText: { letterSpacing: 8, fontWeight: 'bold' },
});
