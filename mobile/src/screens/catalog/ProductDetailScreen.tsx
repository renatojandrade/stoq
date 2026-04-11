import React from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Button,
  Card,
  Chip,
  Divider,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { CatalogStackParamList } from '../../navigation/CatalogNavigator';
import { useGetProductQuery, useDeleteProductMutation } from '../../store/api/catalogApi';

type Props = NativeStackScreenProps<CatalogStackParamList, 'ProductDetail'>;

export default function ProductDetailScreen({ route, navigation }: Props) {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const { productId } = route.params;

  const { data: product, isLoading, isError } = useGetProductQuery(productId);
  const [deleteProduct, { isLoading: isDeleting }] = useDeleteProductMutation();

  async function handleDelete() {
    await deleteProduct(productId);
    navigation.goBack();
  }

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (isError || !product) {
    return (
      <View style={styles.center}>
        <Text>{t('catalog.detail.notFound')}</Text>
      </View>
    );
  }

  return (
    <ScrollView
      style={{ backgroundColor: colors.background }}
      contentContainerStyle={styles.content}
    >
      <Card style={styles.card}>
        <Card.Title
          title={product.name}
          subtitle={product.brand ?? undefined}
          titleVariant="headlineSmall"
        />
        <Divider />
        <Card.Content style={styles.cardContent}>
          <Row label={t('catalog.form.category')} value={product.category} />
          <Row label={t('catalog.form.unit')} value={t(`catalog.units.${product.unit}`)} />
          <Row label={t('catalog.form.minQuantity')} value={product.minQuantity.toString()} />
          {product.barcode && (
            <Row label={t('catalog.form.barcode')} value={product.barcode} />
          )}
        </Card.Content>
        {product.imageUrl && (
          <Card.Cover source={{ uri: product.imageUrl }} style={styles.cover} />
        )}
      </Card>

      <View style={styles.chips}>
        <Chip icon="tag-outline">{product.category}</Chip>
        <Chip icon="scale">{t(`catalog.units.${product.unit}`)}</Chip>
      </View>

      <View style={styles.buttonRow}>
        <Button
          mode="outlined"
          icon="pencil"
          onPress={() => navigation.navigate('ProductForm', { product })}
          style={styles.button}
        >
          {t('catalog.detail.edit')}
        </Button>
        <Button
          mode="outlined"
          icon="trash-can-outline"
          textColor={colors.error}
          onPress={handleDelete}
          loading={isDeleting}
          style={styles.button}
        >
          {t('catalog.detail.delete')}
        </Button>
      </View>
    </ScrollView>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  const { colors } = useTheme();
  return (
    <View style={styles.row}>
      <Text variant="labelMedium" style={{ color: colors.onSurfaceVariant }}>
        {label}
      </Text>
      <Text variant="bodyLarge">{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  content: { padding: 16, gap: 16 },
  card: {},
  cardContent: { gap: 12, paddingTop: 12 },
  cover: { marginTop: 12 },
  chips: { flexDirection: 'row', gap: 8, flexWrap: 'wrap' },
  buttonRow: { flexDirection: 'row', gap: 12 },
  button: { flex: 1 },
  row: { gap: 2 },
});
