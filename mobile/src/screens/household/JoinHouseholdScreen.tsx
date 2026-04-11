import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, StyleSheet } from 'react-native';
import { Button, HelperText, Text, TextInput } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useDispatch } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { HouseholdStackParamList } from '../../navigation/HouseholdSetupNavigator';
import { useJoinHouseholdMutation } from '../../store/api/authApi';
import { setHousehold } from '../../store/authSlice';

type Props = NativeStackScreenProps<HouseholdStackParamList, 'JoinHousehold'>;

export default function JoinHouseholdScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const dispatch = useDispatch();
  const [joinHousehold, { isLoading }] = useJoinHouseholdMutation();

  const [inviteCode, setInviteCode] = useState('');
  const [error, setError] = useState<string | null>(null);

  async function handleJoin() {
    setError(null);
    const code = inviteCode.trim().toUpperCase();
    if (!code) { setError(t('household.join.codeRequired')); return; }
    if (code.length !== 6) { setError(t('household.join.codeLength')); return; }

    try {
      const household = await joinHousehold({ inviteCode: code }).unwrap();
      dispatch(setHousehold(household.id));
    } catch (err: unknown) {
      const message = (err as { data?: { detail?: string } })?.data?.detail;
      setError(message ?? t('auth.errors.unknown'));
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <Text variant="headlineMedium" style={styles.title}>{t('household.join.title')}</Text>

      <TextInput
        label={t('household.join.inviteCode')}
        value={inviteCode}
        onChangeText={setInviteCode}
        placeholder={t('household.join.codePlaceholder')}
        autoCapitalize="characters"
        maxLength={6}
        style={styles.input}
        mode="outlined"
      />

      {error && <HelperText type="error">{error}</HelperText>}

      <Button
        mode="contained"
        onPress={handleJoin}
        loading={isLoading}
        disabled={isLoading}
        style={styles.button}
      >
        {t('household.join.submit')}
      </Button>

      <Button mode="text" onPress={() => navigation.goBack()} style={styles.backButton}>
        {t('common.cancel')}
      </Button>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center', backgroundColor: '#fff' },
  title: { marginBottom: 24 },
  input: { marginBottom: 12 },
  button: { marginTop: 8 },
  backButton: { marginTop: 4 },
});
