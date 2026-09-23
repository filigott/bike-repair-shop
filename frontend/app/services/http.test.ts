import { afterEach, describe, expect, it, vi } from "vitest";
import { ProblemCode, ValidationCode } from "~/types/problems";
import { actionFailureFrom } from "./action-errors";
import { listOrders, placeOrder } from "./endpoints/orders";
import { ApiError } from "./http";

function stubFetch(status: number, body: unknown) {
  const spy = vi.fn(
    async (_input: Request | URL | string) =>
      new Response(JSON.stringify(body), {
        status,
        headers: { "content-type": "application/json" },
      }),
  );
  vi.stubGlobal("fetch", spy);
  return spy;
}

function requestedUrl(spy: ReturnType<typeof stubFetch>): URL {
  const input = spy.mock.calls[0][0];
  return new URL(input instanceof Request ? input.url : String(input));
}

describe("the API client", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("returns the JSON body on success", async () => {
    // Arrange
    stubFetch(200, { items: [], page: 1, pageSize: 20, totalCount: 0 });

    // Act
    const result = await listOrders();

    // Assert
    expect(result).toEqual({ items: [], page: 1, pageSize: 20, totalCount: 0 });
  });

  it("serialises the query parameters onto the path", async () => {
    // Arrange
    const spy = stubFetch(200, {
      items: [],
      page: 2,
      pageSize: 20,
      totalCount: 0,
    });

    // Act
    await listOrders({ search: "Ada", page: 2, pageSize: 20 });

    // Assert
    const url = requestedUrl(spy);
    expect(url.pathname).toBe("/api/orders");
    expect(url.searchParams.get("search")).toBe("Ada");
    expect(url.searchParams.get("page")).toBe("2");
  });

  it("throws an ApiError carrying the problem details on failure", async () => {
    // Arrange
    stubFetch(400, {
      title: "Validation failed",
      status: 400,
      errors: { email: ["'Email' must be a valid email address."] },
      errorCodes: { email: ["InvalidEmail", "SomethingNew"] },
      code: "InvalidRequest",
    });

    // Act
    const act = placeOrder({
      customerName: "Ada",
      phoneNumber: "+4712345678",
      email: "not-an-email",
      bikeBrand: "Bianchi",
      serviceType: "WheelAdjustment",
      dueDate: "2026-09-25",
      notes: null,
    });

    // Assert
    await expect(act).rejects.toMatchObject({
      status: 400,
      code: ProblemCode.InvalidRequest,
      fieldErrors: {
        email: [ValidationCode.InvalidEmail, ValidationCode.Invalid],
      },
    });
  });
});

describe("actionFailureFrom", () => {
  it("turns a 4xx ApiError into a failure carrying its code", () => {
    // Arrange
    const error = new ApiError(422, {
      title: "Unprocessable",
      detail: "Too late",
      code: "DueDateInPast",
    });

    // Act
    const failure = actionFailureFrom(error);

    // Assert
    expect(failure).toMatchObject({
      status: 422,
      code: ProblemCode.DueDateInPast,
      fieldErrors: {},
    });
  });

  it("reads a code it does not know, or none at all, as unknown", () => {
    // Act & Assert
    expect(
      actionFailureFrom(new ApiError(400, { title: "Bad", code: "Melted" })),
    ).toMatchObject({ code: ProblemCode.Unknown });
    expect(
      actionFailureFrom(new ApiError(400, { title: "Bad" })),
    ).toMatchObject({ code: ProblemCode.Unknown });
  });

  it("rethrows anything that is not a 4xx", () => {
    // Act & Assert
    expect(() =>
      actionFailureFrom(new ApiError(500, { title: "Boom" })),
    ).toThrow(ApiError);
    expect(() => actionFailureFrom(new TypeError("offline"))).toThrow(
      TypeError,
    );
  });
});
