import { beforeEach, describe, expect, it, vi } from "vitest";
import { startOrder } from "~/services/endpoints/orders";
import { runOrderCommand } from "~/services/order-commands";
import { OrderIntent, orderIntentOf } from "~/types/intents/orders";

vi.mock("~/services/endpoints/orders", () => ({
  startOrder: vi.fn(),
  completeOrder: vi.fn(),
  cancelOrder: vi.fn(),
  reopenOrder: vi.fn(),
  correctOrderDetails: vi.fn(),
  changeOrderService: vi.fn(),
  rescheduleOrder: vi.fn(),
  deleteOrder: vi.fn(),
}));

const ORDER_ID = "0199c3a7-8c9b-7628-bb49-ebe3ff2b7814";

function formWith(fields: Record<string, string>): FormData {
  const form = new FormData();

  for (const [name, value] of Object.entries(fields)) {
    form.set(name, value);
  }

  return form;
}

describe("runOrderCommand", () => {
  beforeEach(() => {
    vi.mocked(startOrder)
      .mockReset()
      .mockResolvedValue({} as never);
  });

  it("calls the endpoint the action names", async () => {
    // Act
    await runOrderCommand(
      ORDER_ID,
      OrderIntent.Transition,
      formWith({ action: "Start" }),
    );

    // Assert
    expect(startOrder).toHaveBeenCalledWith(ORDER_ID);
  });

  it("refuses an action the lifecycle does not have", () => {
    // Act & Assert
    expect(() =>
      runOrderCommand(
        ORDER_ID,
        OrderIntent.Transition,
        formWith({ action: "Dismantle" }),
      ),
    ).toThrow("Unknown order action: Dismantle");
  });

  it("refuses an action borrowed from the object prototype", () => {
    // Act & Assert
    expect(() =>
      runOrderCommand(
        ORDER_ID,
        OrderIntent.Transition,
        formWith({ action: "toString" }),
      ),
    ).toThrow("Unknown order action: toString");
  });
});

describe("orderIntentOf", () => {
  it("reads back an intent the app posts", () => {
    // Act & Assert
    expect(orderIntentOf("orders.transition")).toBe(OrderIntent.Transition);
  });

  it("refuses anything else", () => {
    // Act & Assert
    expect(() => orderIntentOf("orders.melt")).toThrow(
      "Unknown order intent: orders.melt",
    );
  });
});
