import { describe, expect, it } from "vitest";
import { OrderIntent } from "~/types/intents/orders";
import {
  editCommandOf,
  OrderEditParam,
  ordersHref,
  ordersQueryOf,
} from "~/types/params/orders";

describe("editCommandOf", () => {
  it("names the command an edit param opens", () => {
    // Act & Assert
    expect(editCommandOf(OrderEditParam.DueDate)).toBe(OrderIntent.Reschedule);
  });

  it.each([null, "", "toString", "constructor", "__proto__"])(
    "opens nothing for %j",
    (value) => {
      // Act & Assert
      expect(editCommandOf(value)).toBeNull();
    },
  );
});

describe("ordersQueryOf", () => {
  it("reads a trimmed search and a page", () => {
    // Act
    const query = ordersQueryOf(
      new URLSearchParams("search=%20Hopper%20&page=3"),
    );

    // Assert
    expect(query).toEqual({ search: "Hopper", page: 3 });
  });

  it.each(["", "0", "-2", "1.5", "two"])(
    "falls back to the first page for %j",
    (page) => {
      // Act & Assert
      expect(ordersQueryOf(new URLSearchParams({ page })).page).toBe(1);
    },
  );

  it("reads a missing search as empty", () => {
    // Act & Assert
    expect(ordersQueryOf(new URLSearchParams()).search).toBe("");
  });
});

describe("ordersHref", () => {
  it("carries the search into the page link", () => {
    // Act & Assert
    expect(ordersHref({ search: "Hopper", page: 2 })).toBe(
      "?search=Hopper&page=2",
    );
  });

  it("leaves an empty search out", () => {
    // Act & Assert
    expect(ordersHref({ search: "", page: 2 })).toBe("?page=2");
  });

  it("reads back the query it wrote", () => {
    // Arrange
    const query = { search: "Ada Lovelace", page: 4 };

    // Act & Assert
    expect(ordersQueryOf(new URLSearchParams(ordersHref(query)))).toEqual(
      query,
    );
  });
});
