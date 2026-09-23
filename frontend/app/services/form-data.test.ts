import { describe, expect, it } from "vitest";
import { optionalIntegerOf } from "~/services/form-data";

function formWith(value: string): FormData {
  const form = new FormData();
  form.set("version", value);
  return form;
}

describe("optionalIntegerOf", () => {
  it("reads a whole number", () => {
    // Act & Assert
    expect(optionalIntegerOf(formWith("4021"), "version")).toBe(4021);
  });

  it.each(["", "   ", "7.5", "seven"])(
    "refuses %j rather than guessing a number",
    (value) => {
      // Act & Assert
      expect(optionalIntegerOf(formWith(value), "version")).toBeNull();
    },
  );
});
