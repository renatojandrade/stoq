import { useEffect, useRef } from 'react';
import * as Device from 'expo-device';
import * as Notifications from 'expo-notifications';
import Constants from 'expo-constants';
import type { Subscription } from 'expo-notifications';

const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
  }),
});

async function registerAndSendToken(accessToken: string): Promise<void> {
  if (!Device.isDevice) return;

  const { status: existing } = await Notifications.getPermissionsAsync();
  let finalStatus = existing;

  if (existing !== 'granted') {
    const { status } = await Notifications.requestPermissionsAsync();
    finalStatus = status;
  }

  if (finalStatus !== 'granted') return;

  const projectId = Constants.expoConfig?.extra?.eas?.projectId as string | undefined;
  const tokenData = await Notifications.getExpoPushTokenAsync({ projectId });

  await fetch(`${BASE_URL}/api/notifications/token`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({ token: tokenData.data }),
  });
}

export function usePushNotifications(
  accessToken: string | null,
  onNotificationTap?: (data: Record<string, string>) => void,
): void {
  const responseListenerRef = useRef<Subscription | null>(null);

  useEffect(() => {
    if (!accessToken) return;

    registerAndSendToken(accessToken).catch(() => {
      // Non-critical — push notifications are best-effort
    });

    responseListenerRef.current = Notifications.addNotificationResponseReceivedListener((response) => {
      const data = response.notification.request.content.data as Record<string, string> | undefined;
      if (data && onNotificationTap) {
        onNotificationTap(data);
      }
    });

    return () => {
      if (responseListenerRef.current) {
        Notifications.removeNotificationSubscription(responseListenerRef.current);
      }
    };
  }, [accessToken, onNotificationTap]);
}
