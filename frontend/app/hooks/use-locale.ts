import { useTranslation } from "react-i18next";
import { type Locale, locales, toLocale } from "~/types/locale";

export function useLocale() {
  const { i18n } = useTranslation();
  return {
    locale: toLocale(i18n.resolvedLanguage),
    locales,
    setLocale: (locale: Locale) => i18n.changeLanguage(locale),
  };
}
