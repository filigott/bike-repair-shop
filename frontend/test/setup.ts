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

// jsdom ships <dialog> without its modal behaviour, so the native methods are stubbed here.
HTMLDialogElement.prototype.showModal = function showModal() {
  this.open = true;
};

HTMLDialogElement.prototype.close = function close() {
  this.open = false;
  this.dispatchEvent(new Event("close"));
};
