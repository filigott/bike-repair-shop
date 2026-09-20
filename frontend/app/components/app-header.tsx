import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { useLocale } from "~/hooks/use-locale";
import { toLocale } from "~/types/locale";

export function AppHeader() {
  const { t } = useTranslation();
  const { locale, locales, setLocale } = useLocale();

  return (
    <header className="navbar bg-base-200 px-4">
      <Link to="/" className="text-xl font-bold">
        {t("app.title")}
      </Link>
      <select
        className="select select-sm ml-auto w-auto"
        aria-label={t("common.language")}
        value={locale}
        onChange={(event) => setLocale(toLocale(event.target.value))}
      >
        {locales.map((option) => (
          <option key={option} value={option}>
            {t(`locale.${option}`)}
          </option>
        ))}
      </select>
    </header>
  );
}
