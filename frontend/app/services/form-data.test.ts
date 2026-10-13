import { describe, expect, it } from "vitest";
import { optionalIntegerOf, serviceTypeOf } from "~/services/form-data";

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

describe("serviceTypeOf", () => {
  it("reads a service the shop offers", () => {
    // Arrange
    const form = new FormData();
    form.set("serviceType", "BrakeMaintenance");

    // Act & Assert
    expect(serviceTypeOf(form, "serviceType")).toBe("BrakeMaintenance");
  });

  it.each(["", "Painting", "toString"])("refuses %j", (value) => {
    // Arrange
    const form = new FormData();
    form.set("serviceType", value);

    // Act & Assert
    expect(() => serviceTypeOf(form, "serviceType")).toThrow(
      `Unknown service type: ${value}`,
    );
  });
});
