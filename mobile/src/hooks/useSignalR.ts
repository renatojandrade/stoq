import { useEffect, useRef } from 'react';
import * as SignalR from '@microsoft/signalr';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';

const BASE_URL = process.env['EXPO_PUBLIC_API_URL'] ?? 'http://localhost:5000';

// Module-level singleton — all hook consumers share one connection per session.
let _sharedConnection: SignalR.HubConnection | null = null;

function buildConnection(accessToken: string): SignalR.HubConnection {
  return new SignalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/receipts`, {
      accessTokenFactory: () => accessToken,
    })
    .withAutomaticReconnect()
    .configureLogging(SignalR.LogLevel.Warning)
    .build();
}

export function useSignalR(): SignalR.HubConnection | null {
  const accessToken = useSelector((s: RootState) => s.auth.accessToken);
  const tokenRef = useRef<string | null>(null);

  useEffect(() => {
    if (!accessToken) {
      _sharedConnection?.stop();
      _sharedConnection = null;
      tokenRef.current = null;
      return;
    }

    // Already connected with the same token — nothing to do.
    if (
      _sharedConnection?.state === SignalR.HubConnectionState.Connected &&
      tokenRef.current === accessToken
    ) {
      return;
    }

    // Token changed or not yet connected — rebuild.
    _sharedConnection?.stop();
    tokenRef.current = accessToken;

    const conn = buildConnection(accessToken);
    _sharedConnection = conn;

    conn.start().catch((err: unknown) => {
      console.warn('[SignalR] Failed to connect:', err);
    });

    return () => {
      // Do not stop on component unmount — the singleton outlives any single screen.
    };
  }, [accessToken]);

  return _sharedConnection;
}
