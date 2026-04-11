import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, StyleSheet, View } from 'react-native';
import { Button, HelperText, Text, TextInput } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { AuthStackParamList } from '../../navigation/AuthNavigator';
import { useForgotPasswordMutation } from '../../store/api/authApi';

type Props = NativeStackScreenProps<AuthStackParamList, 'ForgotPassword'>;

export default function ForgotPasswordScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const [forgotPassword, { isLoading }] = useForgotPasswordMutation();

  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(false);

  async function handleSubmit() {
    setError(null);
    if (!email.trim()) { setError(t('auth.validation.emailRequired')); return; }

    try {
      await forgotPassword({ email: email.trim() }).unwrap();
      setSubmitted(true);
    } catch {
      setError(t('auth.errors.networkError'));
    }
  }

  if (submitted) {
    return (
      <View style={styles.container}>
        <View style={styles.inner}>
          <Text variant="headlineMedium" style={styles.title}>
            {t('auth.forgotPassword.successTitle')}
          </Text>
          <Text variant="bodyMedium" style={styles.subtitle}>
            {t('auth.forgotPassword.successSubtitle', { email })}
          </Text>
          <Button
            mode="contained"
            onPress={() => navigation.navigate('ResetPassword', { email })}
            style={styles.submitButton}
          >
            {t('auth.resetPassword.title')}
          </Button>
          <Button mode="text" onPress={() => navigation.navigate('Login')} style={styles.backButton}>
            {t('auth.forgotPassword.backToLogin')}
          </Button>
        </View>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <View style={styles.inner}>
        <Text variant="headlineMedium" style={styles.title}>{t('auth.forgotPassword.title')}</Text>
        <Text variant="bodyMedium" style={styles.subtitle}>{t('auth.forgotPassword.subtitle')}</Text>

        <TextInput
          label={t('auth.forgotPassword.email')}
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
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
          {t('auth.forgotPassword.submit')}
        </Button>

        <Button mode="text" onPress={() => navigation.navigate('Login')} style={styles.backButton}>
          {t('auth.forgotPassword.backToLogin')}
        </Button>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  inner: { flex: 1, padding: 24, justifyContent: 'center' },
  title: { marginBottom: 4 },
  subtitle: { marginBottom: 24, opacity: 0.6 },
  input: { marginBottom: 12 },
  submitButton: { marginTop: 8 },
  backButton: { marginTop: 8 },
});
