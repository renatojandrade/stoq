import React from 'react';
import { StyleSheet, View } from 'react-native';
import { Button, Text } from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { HouseholdStackParamList } from '../../navigation/HouseholdSetupNavigator';

type Props = NativeStackScreenProps<HouseholdStackParamList, 'HouseholdSetup'>;

export default function HouseholdSetupScreen({ navigation }: Props) {
  const { t } = useTranslation();

  return (
    <View style={styles.container}>
      <Text variant="headlineMedium" style={styles.title}>
        {t('household.setup.title')}
      </Text>
      <Text variant="bodyMedium" style={styles.subtitle}>
        {t('household.setup.subtitle')}
      </Text>

      <Button
        mode="contained"
        onPress={() => navigation.navigate('CreateHousehold')}
        style={styles.button}
        icon="home-plus"
      >
        {t('household.create.submit')}
      </Button>

      <Button
        mode="outlined"
        onPress={() => navigation.navigate('JoinHousehold')}
        style={styles.button}
        icon="account-multiple-plus"
      >
        {t('household.join.submit')}
      </Button>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center', backgroundColor: '#fff' },
  title: { marginBottom: 8, textAlign: 'center' },
  subtitle: { marginBottom: 40, textAlign: 'center', opacity: 0.6 },
  button: { marginBottom: 12 },
});
