export enum Locale {
  En = "en",
  Nb = "nb",
}

export const locales = Object.values(Locale);

export function toLocale(value: string | undefined): Locale {
  return locales.find((locale) => locale === value) ?? Locale.En;
}
