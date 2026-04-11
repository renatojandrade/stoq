import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, StyleSheet, View } from 'react-native';
import { Button, Divider, HelperText, Text, TextInput } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useDispatch } from 'react-redux';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { AuthStackParamList } from '../../navigation/AuthNavigator';
import { useLoginMutation } from '../../store/api/authApi';
import { setCredentials } from '../../store/authSlice';
import { useGoogleAuth } from '../../hooks/useGoogleAuth';

type Props = NativeStackScreenProps<AuthStackParamList, 'Login'>;

export default function LoginScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const dispatch = useDispatch();
  const [login, { isLoading }] = useLoginMutation();
  const { signInWithGoogle, isReady: googleReady } = useGoogleAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [socialLoading, setSocialLoading] = useState(false);

  async function handleLogin() {
    setError(null);
    if (!email.trim()) { setError(t('auth.validation.emailRequired')); return; }
    if (!password) { setError(t('auth.validation.passwordRequired')); return; }

    try {
      const result = await login({ email: email.trim(), password }).unwrap();
      dispatch(setCredentials(result));
    } catch {
      setError(t('auth.errors.invalidCredentials'));
    }
  }

  async function handleGoogleSignIn() {
    setError(null);
    setSocialLoading(true);
    try {
      const err = await signInWithGoogle();
      if (err) { setError(err); }
    } finally {
      setSocialLoading(false);
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <View style={styles.inner}>
        <Text variant="headlineMedium" style={styles.title}>{t('auth.login.title')}</Text>
        <Text variant="bodyMedium" style={styles.subtitle}>{t('auth.login.subtitle')}</Text>

        <TextInput
          label={t('auth.login.email')}
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
          style={styles.input}
          mode="outlined"
        />
        <TextInput
          label={t('auth.login.password')}
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
          mode="text"
          onPress={() => navigation.navigate('ForgotPassword')}
          style={styles.forgotButton}
        >
          {t('auth.login.forgotPassword')}
        </Button>

        <Button
          mode="contained"
          onPress={handleLogin}
          loading={isLoading}
          disabled={isLoading || socialLoading}
          style={styles.submitButton}
        >
          {t('auth.login.submit')}
        </Button>

        <Divider style={styles.divider} />

        <Button
          mode="outlined"
          icon="google"
          onPress={handleGoogleSignIn}
          loading={socialLoading}
          disabled={!googleReady || isLoading || socialLoading}
          style={styles.socialButton}
        >
          {t('auth.login.continueWithGoogle')}
        </Button>

        <View style={styles.row}>
          <Text variant="bodyMedium">{t('auth.login.noAccount')} </Text>
          <Button mode="text" compact onPress={() => navigation.navigate('Register')}>
            {t('auth.login.register')}
          </Button>
        </View>
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
  forgotButton: { alignSelf: 'flex-end', marginBottom: 8 },
  submitButton: { marginTop: 8 },
  divider: { marginVertical: 20 },
  socialButton: { marginBottom: 8 },
  row: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', marginTop: 16 },
});
