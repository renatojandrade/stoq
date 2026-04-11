import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, StyleSheet } from 'react-native';
import { Button, HelperText, Text, TextInput } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { AuthStackParamList } from '../../navigation/AuthNavigator';
import { useResetPasswordMutation } from '../../store/api/authApi';

type Props = NativeStackScreenProps<AuthStackParamList, 'ResetPassword'>;

export default function ResetPasswordScreen({ navigation, route }: Props) {
  const { t } = useTranslation();
  const [resetPassword, { isLoading }] = useResetPasswordMutation();

  const email = route.params?.email ?? '';
  const [otpCode, setOtpCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit() {
    setError(null);
    if (!otpCode.trim()) { setError(t('auth.validation.otpRequired')); return; }
    if (otpCode.trim().length !== 6) { setError(t('auth.validation.otpLength')); return; }
    if (!newPassword) { setError(t('auth.validation.passwordRequired')); return; }
    if (newPassword.length < 8) { setError(t('auth.validation.passwordMinLength')); return; }

    try {
      await resetPassword({ email, otpCode: otpCode.trim(), newPassword }).unwrap();
      navigation.navigate('Login');
    } catch {
      setError(t('auth.errors.invalidOtp'));
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <Text variant="headlineMedium" style={styles.title}>{t('auth.resetPassword.title')}</Text>
      <Text variant="bodyMedium" style={styles.subtitle}>{t('auth.resetPassword.subtitle')}</Text>

      <TextInput
        label={t('auth.resetPassword.otpCode')}
        value={otpCode}
        onChangeText={setOtpCode}
        keyboardType="number-pad"
        maxLength={6}
        style={styles.input}
        mode="outlined"
      />
      <TextInput
        label={t('auth.resetPassword.newPassword')}
        value={newPassword}
        onChangeText={setNewPassword}
        secureTextEntry={!showPassword}
        right={
          <TextInput.Icon
            icon={showPassword ? 'eye-off' : 'eye'}
            onPress={() => setShowPassword((v) => !v)}
          />
        }
        style={styles.input}
        mode="outlined"
      />

      {error && <HelperText type="error">{error}</HelperText>}

      <Button
        mode="contained"
        onPress={handleSubmit}
        loading={isLoading}
        disabled={isLoading}
        style={styles.submitButton}
      >
        {t('auth.resetPassword.submit')}
      </Button>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff', padding: 24, justifyContent: 'center' },
  title: { marginBottom: 4 },
  subtitle: { marginBottom: 24, opacity: 0.6 },
  input: { marginBottom: 12 },
  submitButton: { marginTop: 8 },
});
