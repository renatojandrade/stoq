import { useCallback } from 'react';
import * as WebBrowser from 'expo-web-browser';
import * as Google from 'expo-auth-session/providers/google';
import { makeRedirectUri } from 'expo-auth-session';
import { useDispatch } from 'react-redux';
import type { AppDispatch } from '../store';
import { setCredentials } from '../store/authSlice';
import { useSocialAuthMutation } from '../store/api/authApi';

WebBrowser.maybeCompleteAuthSession();

// Replace these with the client IDs from Google Cloud Console.
const ANDROID_CLIENT_ID = process.env['EXPO_PUBLIC_GOOGLE_ANDROID_CLIENT_ID'] ?? '';
const WEB_CLIENT_ID = process.env['EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID'] ?? '';

export function useGoogleAuth() {
  const dispatch = useDispatch<AppDispatch>();
  const [socialAuth] = useSocialAuthMutation();

  const [request, response, promptAsync] = Google.useAuthRequest({
    androidClientId: ANDROID_CLIENT_ID,
    webClientId: WEB_CLIENT_ID,
    redirectUri: makeRedirectUri({ scheme: 'stoq' }),
  });

  const signInWithGoogle = useCallback(async (): Promise<string | null> => {
    const result = await promptAsync();

    if (result.type !== 'success') {
      return null;
    }

    const idToken = result.authentication?.idToken;
    if (!idToken) {
      return 'Google sign-in did not return an ID token.';
    }

    try {
      const authResult = await socialAuth({
        provider: 'Google',
        providerToken: idToken,
      }).unwrap();

      dispatch(setCredentials(authResult));
      return null;
    } catch {
      return 'Google sign-in failed. Please try again.';
    }
  }, [promptAsync, socialAuth, dispatch]);

  return { signInWithGoogle, isReady: !!request };
}
