import "@testing-library/jest-dom/vitest";
import { beforeEach } from "vitest";
import i18n from "~/i18n";
import { Locale } from "~/types/locale";

beforeEach(async () => {
  await i18n.changeLanguage(Locale.En);
});
