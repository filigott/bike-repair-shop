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
  deleteOrder,
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
  deleteOrder: vi.fn(),
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
    { path: "/", Component: () => <h1>Orders</h1> },
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
    vi.mocked(deleteOrder).mockReset().mockResolvedValue(undefined);
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

  it("shows no form until an edit command is chosen", async () => {
    // Act
    renderPage();

    // Assert
    await screen.findByRole("heading", { name: "#1042" });
    expect(screen.queryByLabelText("Customer name")).not.toBeInTheDocument();
    expect(screen.queryByLabelText("Service")).not.toBeInTheDocument();
    expect(screen.queryByLabelText("Due date")).not.toBeInTheDocument();
  });

  it("prefills the dialog of the command that was chosen, and only that one", async () => {
    // Arrange
    renderPage();

    // Act
    await userEvent.click(
      await screen.findByRole("button", { name: "Edit details" }),
    );

    // Assert
    expect(screen.getByLabelText("Customer name")).toHaveValue("Ada Lovelace");
    expect(screen.queryByLabelText("Due date")).not.toBeInTheDocument();
  });

  it("sends only the correct-details command when the details form is saved", async () => {
    // Arrange
    renderPage();
    await userEvent.click(
      await screen.findByRole("button", { name: "Edit details" }),
    );
    const name = screen.getByLabelText("Customer name");
    await userEvent.clear(name);
    await userEvent.type(name, "Grace Hopper");

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Save" }));

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
    await userEvent.click(
      await screen.findByRole("button", { name: "Change service" }),
    );
    await userEvent.selectOptions(
      screen.getByLabelText("Service"),
      "WheelAdjustment",
    );

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Save" }));

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
    await userEvent.click(
      await screen.findByRole("button", { name: "Reschedule" }),
    );
    const dueDate = screen.getByLabelText("Due date");
    await userEvent.clear(dueDate);
    await userEvent.type(dueDate, "2026-10-02");

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Save" }));

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
    await userEvent.click(
      await screen.findByRole("button", { name: "Reschedule" }),
    );
    const dueDate = screen.getByLabelText("Due date");
    await userEvent.clear(dueDate);
    await userEvent.type(dueDate, "2020-01-01");

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Save" }));

    // Assert
    expect(
      await screen.findByText("The due date cannot be before today."),
    ).toBeInTheDocument();
    expect(screen.getByLabelText("Due date")).toBeInTheDocument();
  });

  it("closes the dialog once the command is accepted", async () => {
    // Arrange
    renderPage();
    await userEvent.click(
      await screen.findByRole("button", { name: "Edit details" }),
    );

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Save" }));

    // Assert
    await waitFor(() =>
      expect(screen.queryByLabelText("Customer name")).not.toBeInTheDocument(),
    );
  });

  it("offers no commands for an order the shop can no longer change", async () => {
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
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "A Completed order can no longer be changed.",
    );
    expect(
      screen.queryByRole("button", { name: "Edit details" }),
    ).not.toBeInTheDocument();
  });

  it("still shows what a closed order holds, now that no form carries it", async () => {
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
    expect(await screen.findByText("+4712345678")).toBeInTheDocument();
    expect(screen.getByText("ada@example.com")).toBeInTheDocument();
    expect(screen.getByText("Bianchi")).toBeInTheDocument();
    expect(screen.getByText("Squeaks when braking")).toBeInTheDocument();
  });

  it("names the order in the delete confirmation and returns to the list", async () => {
    // Arrange
    renderPage();
    await userEvent.click(
      await screen.findByRole("button", { name: "Delete" }),
    );
    expect(
      screen.getByText(
        "Delete order #1042 for Ada Lovelace? This cannot be undone.",
      ),
    ).toBeInTheDocument();

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Delete order" }));

    // Assert
    await waitFor(() => expect(deleteOrder).toHaveBeenCalledWith(ORDER_ID));
    expect(
      await screen.findByRole("heading", { name: "Orders" }),
    ).toBeInTheDocument();
  });

  it("still offers delete on a closed order, which removes a record rather than moving a state", async () => {
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
    expect(
      await screen.findByRole("button", { name: "Delete" }),
    ).toBeInTheDocument();
  });

  it("leads back to the list", async () => {
    // Act
    renderPage();

    // Assert
    expect(
      await screen.findByRole("link", { name: "Back to orders" }),
    ).toHaveAttribute("href", "/");
  });

  it("keeps the notes row when there are none, with nothing in the value", async () => {
    // Arrange
    vi.mocked(getOrder).mockResolvedValue(
      order({
        status: "Cancelled",
        isEditable: false,
        allowedActions: [],
        allowedTransitions: [],
        notes: null,
      }),
    );

    // Act
    renderPage();

    // Assert
    await screen.findByText("+4712345678");
    expect(screen.getByText("Notes").nextElementSibling).toBeEmptyDOMElement();
  });
});
