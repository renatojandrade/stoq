import React, { useCallback, useRef, useState } from 'react';
import { StyleSheet, View } from 'react-native';
import { ActivityIndicator, Button, Snackbar, Text, useTheme } from 'react-native-paper';
import { CameraView, useCameraPermissions } from 'expo-camera';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { ShoppingStackParamList } from '../../navigation/ShoppingNavigator';
import { useParseReceiptPhotoMutation, useParseNfeReceiptMutation } from '../../store/api/receiptsApi';

type Props = NativeStackScreenProps<ShoppingStackParamList, 'ReceiptScan'>;

function isNfeUrl(url: string): boolean {
  try {
    const { hostname } = new URL(url);
    return (
      hostname.includes('sefaz') ||
      hostname.includes('fazenda.gov.br') ||
      hostname.includes('nfce') ||
      hostname.includes('nfe.') ||
      hostname.includes('.nfe')
    );
  } catch {
    return false;
  }
}

export default function ReceiptScanScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [permission, requestPermission] = useCameraPermissions();
  const [nfeScanned, setNfeScanned] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const cameraRef = useRef<CameraView>(null);

  const [parsePhoto, { isLoading: isParsingPhoto }] = useParseReceiptPhotoMutation();
  const [parseNfe, { isLoading: isSubmittingNfe }] = useParseNfeReceiptMutation();

  const isParsing = isParsingPhoto || isSubmittingNfe || nfeScanned;

  const handleCapture = useCallback(async () => {
    if (isParsing || !cameraRef.current) return;
    setError(null);

    try {
      const photo = await cameraRef.current.takePictureAsync({ base64: true, quality: 0.7 });
      if (!photo?.base64) return;

      const result = await parsePhoto({
        imageBase64: photo.base64,
        contentType: 'image/jpeg',
      }).unwrap();

      navigation.replace('ReceiptConfirm', {
        items: result.items,
        retailerName: result.retailerName,
      });
    } catch {
      setError(t('shopping.receipt.parseError'));
    }
  }, [isParsing, parsePhoto, navigation, t]);

  const handleQrCodeScanned = useCallback(
    async ({ data }: { data: string }) => {
      if (nfeScanned || isParsing) return;
      if (!isNfeUrl(data)) return;

      setNfeScanned(true);
      setError(null);

      try {
        await parseNfe({ qrCodeUrl: data }).unwrap();

        // Job accepted — go back to scan list where status will update via polling/SignalR.
        navigation.goBack();
      } catch (err: unknown) {
        const status = (err as { status?: number })?.status;
        setError(
          status === 429
            ? t('shopping.receipt.tooManyScans')
            : t('shopping.receipt.nfeError'),
        );
        setNfeScanned(false);
      }
    },
    [nfeScanned, isParsing, parseNfe, navigation, t],
  );

  if (!permission) {
    return (
      <View style={[styles.center, { backgroundColor: colors.background }]}>
        <ActivityIndicator />
      </View>
    );
  }

  if (!permission.granted) {
    return (
      <View style={[styles.center, { backgroundColor: colors.background }]}>
        <Text variant="bodyLarge" style={styles.permissionText}>
          {t('shopping.receipt.permissionRequired')}
        </Text>
        <Button mode="contained" onPress={requestPermission}>
          {t('shopping.receipt.grantPermission')}
        </Button>
        <Button mode="text" onPress={() => navigation.goBack()}>
          {t('common.cancel')}
        </Button>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <CameraView
        ref={cameraRef}
        style={StyleSheet.absoluteFillObject}
        facing="back"
        barcodeScannerSettings={{ barcodeTypes: ['qr'] }}
        onBarcodeScanned={!nfeScanned && !isParsing ? handleQrCodeScanned : undefined}
      />

      <View style={[styles.bottomBar, { backgroundColor: 'rgba(0,0,0,0.55)' }]}>
        {isParsing ? (
          <>
            <ActivityIndicator color="white" />
            <Text style={styles.hint}>
              {isSubmittingNfe || nfeScanned
                ? t('shopping.receipt.nfeDetected')
                : t('shopping.receipt.parsing')}
            </Text>
          </>
        ) : (
          <Text style={styles.hint}>{t('shopping.receipt.hint')}</Text>
        )}

        {!isParsing && (
          <Button
            mode="contained"
            onPress={handleCapture}
            style={styles.captureButton}
            icon="camera"
          >
            {t('shopping.receipt.capture')}
          </Button>
        )}

        <Button mode="text" textColor="white" onPress={() => navigation.goBack()}>
          {t('common.cancel')}
        </Button>
      </View>

      <Snackbar
        visible={error !== null}
        onDismiss={() => setError(null)}
        duration={4000}
        action={{ label: t('common.ok'), onPress: () => setError(null) }}
      >
        {error ?? ''}
      </Snackbar>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 16, padding: 24 },
  permissionText: { textAlign: 'center', marginBottom: 8 },
  bottomBar: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    paddingBottom: 48,
    paddingTop: 24,
    alignItems: 'center',
    gap: 12,
  },
  hint: { color: 'white', fontSize: 15, textAlign: 'center', paddingHorizontal: 24 },
  captureButton: { marginTop: 4 },
});
