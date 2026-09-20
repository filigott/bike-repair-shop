import { afterEach, describe, expect, it, vi } from "vitest";
import { actionFailureFrom } from "./action-errors";
import { ApiError, request } from "./http";

function stubFetch(status: number, body: unknown) {
  vi.stubGlobal(
    "fetch",
    vi.fn(
      async () =>
        new Response(JSON.stringify(body), {
          status,
          headers: { "content-type": "application/json" },
        }),
    ),
  );
}

describe("request", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("returns the JSON body on success", async () => {
    // Arrange
    stubFetch(200, { id: "1" });

    // Act
    const result = await request<{ id: string }>("/api/orders");

    // Assert
    expect(result).toEqual({ id: "1" });
  });

  it("throws an ApiError carrying the problem details on failure", async () => {
    // Arrange
    stubFetch(400, {
      title: "Validation failed",
      status: 400,
      errors: { email: ["Invalid"] },
    });

    // Act
    const act = request("/api/orders", { method: "POST", body: "{}" });

    // Assert
    await expect(act).rejects.toMatchObject({
      status: 400,
      fieldErrors: { email: ["Invalid"] },
    });
  });
});

describe("actionFailureFrom", () => {
  it("turns a 4xx ApiError into a failure with field errors", () => {
    // Arrange
    const error = new ApiError(422, {
      title: "Unprocessable",
      detail: "Too late",
    });

    // Act
    const failure = actionFailureFrom(error);

    // Assert
    expect(failure).toMatchObject({
      status: 422,
      detail: "Too late",
      fieldErrors: {},
    });
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
