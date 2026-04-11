import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import * as Localization from 'expo-localization';

import en from './locales/en.json';
import ptBR from './locales/pt-BR.json';

const resources = {
  en: { translation: en },
  'pt-BR': { translation: ptBR },
} as const;

const deviceLocale = Localization.getLocales()[0]?.languageTag ?? 'en';
const supportedLocales = Object.keys(resources);
const lng = supportedLocales.includes(deviceLocale) ? deviceLocale : 'en';

i18n
  .use(initReactI18next)
  .init({
    resources,
    lng,
    fallbackLng: 'en',
    interpolation: { escapeValue: false },
  });

export default i18n;
