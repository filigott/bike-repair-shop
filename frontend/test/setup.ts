import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach, beforeEach } from "vitest";
import i18n from "~/i18n";
import { Locale } from "~/types/locale";

// Vitest runs without globals, so Testing Library's own auto-cleanup never registers.
afterEach(cleanup);

beforeEach(async () => {
  await i18n.changeLanguage(Locale.En);
});
