import { afterAll, beforeAll, describe, expect, it, vi } from "vitest";
import { formatCalendarDay, formatInstant } from "~/services/dates";

beforeAll(() => {
  vi.stubEnv("TZ", "America/New_York");
});

afterAll(() => {
  vi.unstubAllEnvs();
});

describe("formatCalendarDay", () => {
  it("shows the day the API sent, wherever the viewer is", () => {
    // Act & Assert
    expect(formatCalendarDay("2026-03-16", "en")).toBe("Mar 16, 2026");
  });

  it("follows the locale", () => {
    // Act & Assert
    expect(formatCalendarDay("2026-03-16", "nb")).toBe("16. mars 2026");
  });
});

describe("formatInstant", () => {
  it("shows the viewer's day for an instant", () => {
    // Act & Assert
    expect(formatInstant("2026-03-16T02:00:00Z", "en")).toBe("Mar 15, 2026");
  });
});
