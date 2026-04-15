import React, { useState, useCallback } from 'react';
import { StyleSheet, View } from 'react-native';
import { ActivityIndicator, Button, Text, useTheme } from 'react-native-paper';
import { CameraView, useCameraPermissions } from 'expo-camera';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CatalogStackParamList } from '../../navigation/CatalogNavigator';
import { useLazyLookupByBarcodeQuery } from '../../store/api/catalogApi';

type Props = NativeStackScreenProps<CatalogStackParamList, 'BarcodeScanner'>;

export default function BarcodeScanScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const [permission, requestPermission] = useCameraPermissions();
  const [scanned, setScanned] = useState(false);
  const [lookupBarcode] = useLazyLookupByBarcodeQuery();

  const handleBarcodeScanned = useCallback(
    async ({ data }: { data: string }) => {
      if (scanned) return;
      setScanned(true);

      // Always navigate to the form — prefill whatever we find, fallback to barcode only.
      try {
        const result = await lookupBarcode(data).unwrap();
        navigation.replace('ProductForm', {
          prefill: {
            barcode: result.barcode,
            name: result.name,
            brand: result.brand,
            category: result.category ?? undefined,
            imageUrl: result.imageUrl,
          },
        });
      } catch {
        // Not found in Open Food Facts — still open form with just the barcode pre-filled.
        navigation.replace('ProductForm', { prefill: { barcode: data } });
      }
    },
    [scanned, lookupBarcode, navigation],
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
          {t('catalog.barcode.permissionRequired')}
        </Text>
        <Button mode="contained" onPress={requestPermission}>
          {t('catalog.barcode.grantPermission')}
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
        style={StyleSheet.absoluteFillObject}
        facing="back"
        barcodeScannerSettings={{ barcodeTypes: ['ean13', 'ean8', 'upc_a', 'upc_e', 'code128', 'code39'] }}
        onBarcodeScanned={scanned ? undefined : handleBarcodeScanned}
      />

      {/* Viewfinder overlay */}
      <View style={styles.overlay}>
        <View style={[styles.overlayTop, { backgroundColor: 'rgba(0,0,0,0.5)' }]} />
        <View style={styles.overlayMiddle}>
          <View style={{ flex: 1, backgroundColor: 'rgba(0,0,0,0.5)' }} />
          <View style={styles.scanWindow} />
          <View style={{ flex: 1, backgroundColor: 'rgba(0,0,0,0.5)' }} />
        </View>
        <View style={[styles.overlayBottom, { backgroundColor: 'rgba(0,0,0,0.5)' }]}>
          {scanned ? (
            <ActivityIndicator color="white" />
          ) : (
            <Text style={styles.hint}>{t('catalog.barcode.hint')}</Text>
          )}
          <Button
            mode="text"
            textColor="white"
            onPress={() => navigation.goBack()}
            style={styles.cancelButton}
          >
            {t('common.cancel')}
          </Button>
        </View>
      </View>
    </View>
  );
}

const SCAN_WINDOW = 260;

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 16, padding: 24 },
  permissionText: { textAlign: 'center', marginBottom: 8 },
  overlay: { ...StyleSheet.absoluteFillObject },
  overlayTop: { flex: 1 },
  overlayMiddle: { flexDirection: 'row', height: SCAN_WINDOW },
  overlayBottom: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 16 },
  scanWindow: {
    width: SCAN_WINDOW,
    height: SCAN_WINDOW,
    borderWidth: 2,
    borderColor: 'white',
    borderRadius: 8,
  },
  hint: { color: 'white', fontSize: 16, textAlign: 'center', paddingHorizontal: 24 },
  cancelButton: { marginTop: 8 },
});
