import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView, StyleSheet, View } from 'react-native';
import { Button, HelperText, Text, TextInput } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useDispatch } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { AuthStackParamList } from '../../navigation/AuthNavigator';
import { useRegisterMutation } from '../../store/api/authApi';
import { setCredentials } from '../../store/authSlice';

type Props = NativeStackScreenProps<AuthStackParamList, 'Register'>;

function validate(displayName: string, email: string, password: string, t: (k: string) => string) {
  if (!displayName.trim()) return t('auth.validation.displayNameRequired');
  if (!email.trim()) return t('auth.validation.emailRequired');
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) return t('auth.validation.emailInvalid');
  if (!password) return t('auth.validation.passwordRequired');
  if (password.length < 8) return t('auth.validation.passwordMinLength');
  if (!/[A-Z]/.test(password)) return t('auth.validation.passwordUppercase');
  if (!/[0-9]/.test(password)) return t('auth.validation.passwordDigit');
  return null;
}

export default function RegisterScreen({ navigation }: Props) {
  const { t, i18n } = useTranslation();
  const dispatch = useDispatch();
  const [register, { isLoading }] = useRegisterMutation();

  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleRegister() {
    setError(null);
    const validationError = validate(displayName, email, password, t);
    if (validationError) { setError(validationError); return; }

    try {
      const result = await register({
        displayName: displayName.trim(),
        email: email.trim(),
        password,
        preferredLanguage: i18n.language,
      }).unwrap();
      dispatch(setCredentials(result));
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
      <ScrollView contentContainerStyle={styles.inner} keyboardShouldPersistTaps="handled">
        <Text variant="headlineMedium" style={styles.title}>{t('auth.register.title')}</Text>
        <Text variant="bodyMedium" style={styles.subtitle}>{t('auth.register.subtitle')}</Text>

        <TextInput
          label={t('auth.register.displayName')}
          value={displayName}
          onChangeText={setDisplayName}
          style={styles.input}
          mode="outlined"
        />
        <TextInput
          label={t('auth.register.email')}
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
          style={styles.input}
          mode="outlined"
        />
        <TextInput
          label={t('auth.register.password')}
          value={password}
          onChangeText={setPassword}
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
          onPress={handleRegister}
          loading={isLoading}
          disabled={isLoading}
          style={styles.submitButton}
        >
          {t('auth.register.submit')}
        </Button>

        <View style={styles.row}>
          <Text variant="bodyMedium">{t('auth.register.haveAccount')} </Text>
          <Button mode="text" compact onPress={() => navigation.navigate('Login')}>
            {t('auth.register.login')}
          </Button>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  inner: { padding: 24, justifyContent: 'center', flexGrow: 1 },
  title: { marginBottom: 4 },
  subtitle: { marginBottom: 24, opacity: 0.6 },
  input: { marginBottom: 12 },
  submitButton: { marginTop: 8 },
  row: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', marginTop: 16 },
});
