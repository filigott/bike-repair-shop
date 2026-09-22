import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createRoutesStub } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { MaintenanceOrderResponse } from "~/services/endpoints/orders";
import {
  cancelOrder,
  changeOrderService,
  completeOrder,
  correctOrderDetails,
  getOrder,
  reopenOrder,
  rescheduleOrder,
  startOrder,
} from "~/services/endpoints/orders";
import { listServiceTypes } from "~/services/endpoints/service-types";
import { ApiError } from "~/services/http";
import OrderDetail, { clientAction, clientLoader } from "./order-detail";

vi.mock("~/services/endpoints/orders", () => ({
  getOrder: vi.fn(),
  correctOrderDetails: vi.fn(),
  changeOrderService: vi.fn(),
  rescheduleOrder: vi.fn(),
  startOrder: vi.fn(),
  completeOrder: vi.fn(),
  cancelOrder: vi.fn(),
  reopenOrder: vi.fn(),
}));
vi.mock("~/services/endpoints/service-types", () => ({
  listServiceTypes: vi.fn(),
}));

const ORDER_ID = "0199c3a7-8c9b-7628-bb49-ebe3ff2b7814";

function order(
  overrides: Partial<MaintenanceOrderResponse> = {},
): MaintenanceOrderResponse {
  return {
    id: ORDER_ID,
    orderNumber: 1042,
    customerName: "Ada Lovelace",
    phoneNumber: "+4712345678",
    email: "ada@example.com",
    bikeBrand: "Bianchi",
    serviceType: "ChainReplacement",
    dueDate: "2026-09-25",
    notes: "Squeaks when braking",
    status: "Received",
    placedAt: "2026-09-20T08:00:00+00:00",
    isOverdue: false,
    isEditable: true,
    allowedActions: ["Start", "Complete", "Cancel"],
    allowedTransitions: ["InProgress", "Completed", "Cancelled"],
    ...overrides,
  };
}

function renderPage() {
  const Stub = createRoutesStub([
    {
      path: "/orders/:id",
      Component: OrderDetail,
      loader: (args) =>
        clientLoader(args as Parameters<typeof clientLoader>[0]),
      action: (args) =>
        clientAction(args as Parameters<typeof clientAction>[0]),
    },
  ]);
  return render(<Stub initialEntries={[`/orders/${ORDER_ID}`]} />);
}

describe("OrderDetail", () => {
  beforeEach(() => {
    vi.mocked(getOrder).mockReset().mockResolvedValue(order());
    vi.mocked(correctOrderDetails).mockReset().mockResolvedValue(order());
    vi.mocked(changeOrderService).mockReset().mockResolvedValue(order());
    vi.mocked(rescheduleOrder).mockReset().mockResolvedValue(order());
    vi.mocked(startOrder).mockReset().mockResolvedValue(order());
    vi.mocked(completeOrder).mockReset().mockResolvedValue(order());
    vi.mocked(cancelOrder).mockReset().mockResolvedValue(order());
    vi.mocked(reopenOrder).mockReset().mockResolvedValue(order());
    vi.mocked(listServiceTypes).mockResolvedValue([
      { code: "WheelAdjustment", displayName: "Wheel adjustment" },
      { code: "ChainReplacement", displayName: "Chain replacement" },
    ]);
  });

  it("leads with the order number and names the customer below it", async () => {
    // Act
    renderPage();

    // Assert
    expect(
      await screen.findByRole("heading", { name: "#1042" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Ada Lovelace")).toBeInTheDocument();
  });

  it("offers a button per action the order allows and no others", async () => {
    // Act
    renderPage();

    // Assert
    expect(
      await screen.findByRole("button", { name: "Start work" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Complete" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Cancel order" }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Reopen" }),
    ).not.toBeInTheDocument();
  });

  it("offers no actions on a cancelled order", async () => {
    // Arrange
    vi.mocked(getOrder).mockResolvedValue(
      order({
        status: "Cancelled",
        isEditable: false,
        allowedActions: [],
        allowedTransitions: [],
      }),
    );

    // Act
    renderPage();

    // Assert
    await screen.findByRole("heading", { name: "#1042" });
    expect(
      screen.queryByRole("button", { name: "Start work" }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Cancel order" }),
    ).not.toBeInTheDocument();
  });

  it("names the order in the confirmation and sends only that transition", async () => {
    // Arrange
    renderPage();
    await userEvent.click(
      await screen.findByRole("button", { name: "Cancel order" }),
    );
    expect(
      screen.getByText(
        "Cancel order #1042 for Ada Lovelace? A cancelled order cannot be reopened.",
      ),
    ).toBeInTheDocument();

    // Act
    await userEvent.click(
      screen.getAllByRole("button", { name: "Cancel order" })[1],
    );

    // Assert
    await waitFor(() => expect(cancelOrder).toHaveBeenCalledWith(ORDER_ID));
    expect(startOrder).not.toHaveBeenCalled();
    expect(completeOrder).not.toHaveBeenCalled();
    expect(reopenOrder).not.toHaveBeenCalled();
  });

  it("offers reopen and nothing else once an order is completed", async () => {
    // Arrange
    vi.mocked(getOrder).mockResolvedValue(
      order({
        status: "Completed",
        isEditable: false,
        allowedActions: ["Reopen"],
        allowedTransitions: ["InProgress"],
      }),
    );

    // Act
    renderPage();

    // Assert
    expect(
      await screen.findByRole("button", { name: "Reopen" }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Complete" }),
    ).not.toBeInTheDocument();
  });

  it("prefills one form per command from the loaded order", async () => {
    // Act
    renderPage();

    // Assert
    expect(await screen.findByLabelText("Customer name")).toHaveValue(
      "Ada Lovelace",
    );
    expect(screen.getByLabelText("Service")).toHaveValue("ChainReplacement");
    expect(screen.getByLabelText("Due date")).toHaveValue("2026-09-25");
  });

  it("sends only the correct-details command when the details form is saved", async () => {
    // Arrange
    renderPage();
    const name = await screen.findByLabelText("Customer name");
    await userEvent.clear(name);
    await userEvent.type(name, "Grace Hopper");

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Save details" }));

    // Assert
    await waitFor(() =>
      expect(correctOrderDetails).toHaveBeenCalledWith(ORDER_ID, {
        customerName: "Grace Hopper",
        phoneNumber: "+4712345678",
        email: "ada@example.com",
        bikeBrand: "Bianchi",
        notes: "Squeaks when braking",
      }),
    );
    expect(changeOrderService).not.toHaveBeenCalled();
    expect(rescheduleOrder).not.toHaveBeenCalled();
  });

  it("sends only the change-service command when the service form is saved", async () => {
    // Arrange
    renderPage();
    await userEvent.selectOptions(
      await screen.findByLabelText("Service"),
      "WheelAdjustment",
    );

    // Act
    await userEvent.click(
      screen.getByRole("button", { name: "Change service" }),
    );

    // Assert
    await waitFor(() =>
      expect(changeOrderService).toHaveBeenCalledWith(ORDER_ID, {
        serviceType: "WheelAdjustment",
      }),
    );
    expect(correctOrderDetails).not.toHaveBeenCalled();
  });

  it("sends only the reschedule command when the due date form is saved", async () => {
    // Arrange
    renderPage();
    const dueDate = await screen.findByLabelText("Due date");
    await userEvent.clear(dueDate);
    await userEvent.type(dueDate, "2026-10-02");

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Reschedule" }));

    // Assert
    await waitFor(() =>
      expect(rescheduleOrder).toHaveBeenCalledWith(ORDER_ID, {
        dueDate: "2026-10-02",
      }),
    );
    expect(correctOrderDetails).not.toHaveBeenCalled();
  });

  it("shows a rejected field on the form that was submitted", async () => {
    // Arrange
    vi.mocked(rescheduleOrder).mockRejectedValue(
      new ApiError(422, {
        title: "Unprocessable entity",
        status: 422,
        detail: "The due date cannot be before today.",
      }),
    );
    renderPage();
    const dueDate = await screen.findByLabelText("Due date");
    await userEvent.clear(dueDate);
    await userEvent.type(dueDate, "2020-01-01");

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Reschedule" }));

    // Assert
    expect(
      await screen.findByText("The due date cannot be before today."),
    ).toBeInTheDocument();
  });

  it("offers no commands for an order the shop can no longer change", async () => {
    // Arrange
    vi.mocked(getOrder).mockResolvedValue(
      order({ status: "Completed", isEditable: false }),
    );

    // Act
    renderPage();

    // Assert
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "A Completed order can no longer be changed.",
    );
    expect(screen.queryByLabelText("Customer name")).not.toBeInTheDocument();
  });

  it("still shows what a closed order holds, now that no form carries it", async () => {
    // Arrange
    vi.mocked(getOrder).mockResolvedValue(
      order({ status: "Cancelled", isEditable: false }),
    );

    // Act
    renderPage();

    // Assert
    expect(await screen.findByText("+4712345678")).toBeInTheDocument();
    expect(screen.getByText("ada@example.com")).toBeInTheDocument();
    expect(screen.getByText("Bianchi")).toBeInTheDocument();
    expect(screen.getByText("Squeaks when braking")).toBeInTheDocument();
  });

  it("leaves the notes row out when a closed order has none", async () => {
    // Arrange
    vi.mocked(getOrder).mockResolvedValue(
      order({ status: "Cancelled", isEditable: false, notes: null }),
    );

    // Act
    renderPage();

    // Assert
    await screen.findByText("+4712345678");
    expect(screen.queryByText("Notes")).not.toBeInTheDocument();
  });
});
