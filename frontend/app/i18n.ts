import i18next from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";
import en from "./locales/en.json";
import nb from "./locales/nb.json";
import { Locale, locales } from "./types/locale";

export const resources = {
  [Locale.En]: { translation: en },
  [Locale.Nb]: { translation: nb satisfies typeof en },
} as const;

declare module "i18next" {
  interface CustomTypeOptions {
    defaultNS: "translation";
    resources: (typeof resources)[Locale.En];
  }
}

i18next
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: Locale.En,
    supportedLngs: locales,
    load: "languageOnly",
    // Resources are bundled, so a synchronous init keeps the first render out of Suspense.
    initAsync: false,
    interpolation: { escapeValue: false },
    detection: {
      order: ["localStorage", "navigator"],
      caches: ["localStorage"],
    },
  });

export default i18next;
