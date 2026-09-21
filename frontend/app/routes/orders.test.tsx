import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createRoutesStub } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  getDemoDataStatus,
  resetDemoData,
} from "~/services/endpoints/demo-data";
import type {
  MaintenanceOrderSummary,
  PagedOrders,
} from "~/services/endpoints/orders";
import { deleteOrder, listOrders } from "~/services/endpoints/orders";
import { ApiError } from "~/services/http";
import Orders, { clientAction, clientLoader } from "./orders";

vi.mock("~/services/endpoints/orders", () => ({
  listOrders: vi.fn(),
  deleteOrder: vi.fn(),
}));

vi.mock("~/services/endpoints/demo-data", () => ({
  getDemoDataStatus: vi.fn(),
  resetDemoData: vi.fn(),
}));

function summary(
  overrides: Partial<MaintenanceOrderSummary> = {},
): MaintenanceOrderSummary {
  return {
    id: crypto.randomUUID(),
    customerName: "Ada Lovelace",
    phoneNumber: "+4712345678",
    email: "ada@example.com",
    bikeBrand: "Bianchi",
    serviceType: "WheelAdjustment",
    dueDate: "2026-09-25",
    status: "Received",
    isOverdue: false,
    ...overrides,
  };
}

function page(
  items: MaintenanceOrderSummary[],
  totalCount = items.length,
): PagedOrders {
  return { items, page: 1, pageSize: 20, totalCount };
}

function renderOrders(url = "/") {
  const Stub = createRoutesStub([
    {
      path: "/",
      Component: Orders,
      loader: (args) =>
        clientLoader(args as Parameters<typeof clientLoader>[0]),
      action: (args) =>
        clientAction(args as Parameters<typeof clientAction>[0]),
    },
  ]);
  return render(<Stub initialEntries={[url]} />);
}

describe("Orders", () => {
  beforeEach(() => {
    vi.mocked(listOrders).mockReset();
    vi.mocked(deleteOrder).mockReset().mockResolvedValue(undefined);
    vi.mocked(getDemoDataStatus)
      .mockReset()
      .mockResolvedValue({ enabled: false, orderCount: 0 });
    vi.mocked(resetDemoData)
      .mockReset()
      .mockResolvedValue({ enabled: true, orderCount: 26 });
  });

  it("renders a row per order", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(
      page([
        summary({ customerName: "Ada Lovelace" }),
        summary({
          customerName: "Grace Hopper",
          status: "InProgress",
          serviceType: "ChainReplacement",
        }),
      ]),
    );

    // Act
    renderOrders();

    // Assert
    expect(await screen.findByText("Ada Lovelace")).toBeInTheDocument();
    expect(screen.getByText("Grace Hopper")).toBeInTheDocument();
    expect(screen.getByText("In progress")).toBeInTheDocument();
    expect(screen.getByText("Wheel adjustment")).toBeInTheDocument();
    expect(screen.getByText("Chain replacement")).toBeInTheDocument();
  });

  it("marks an overdue order", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(
      page([summary({ isOverdue: true })]),
    );

    // Act
    renderOrders();

    // Assert
    expect(await screen.findByText("Overdue")).toBeInTheDocument();
  });

  it("shows the empty state when nothing matches", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(page([]));

    // Act
    renderOrders();

    // Assert
    expect(await screen.findByText("No orders found")).toBeInTheDocument();
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
  });

  it("sends the typed term as the search parameter", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(page([summary()]));
    renderOrders();
    await screen.findByText("Ada Lovelace");

    // Act
    await userEvent.type(
      screen.getByRole("searchbox", { name: "Search orders" }),
      "Hopper",
    );
    await userEvent.click(screen.getByRole("button", { name: "Search" }));

    // Assert
    await waitFor(() =>
      expect(listOrders).toHaveBeenLastCalledWith({
        search: "Hopper",
        page: 1,
        pageSize: 20,
      }),
    );
  });

  it("asks for the next page and keeps the search term", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(page([summary()], 25));
    renderOrders("/?search=Hopper");
    await screen.findByText("Ada Lovelace");

    // Act
    await userEvent.click(screen.getByRole("link", { name: "Next" }));

    // Assert
    await waitFor(() =>
      expect(listOrders).toHaveBeenLastCalledWith({
        search: "Hopper",
        page: 2,
        pageSize: 20,
      }),
    );
  });

  it("drops the row while the confirmed delete is in flight", async () => {
    // Arrange
    const doomed = summary({ customerName: "Ada Lovelace" });
    vi.mocked(listOrders).mockResolvedValue(
      page([doomed, summary({ customerName: "Grace Hopper" })]),
    );
    let finishDelete = () => {};
    vi.mocked(deleteOrder).mockReturnValue(
      new Promise((resolve) => {
        finishDelete = () => resolve();
      }),
    );
    renderOrders();
    await screen.findByText("Ada Lovelace");
    await userEvent.click(screen.getAllByRole("button", { name: "Delete" })[0]);

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Delete order" }));

    // Assert
    await waitFor(() =>
      expect(screen.queryByText("Ada Lovelace")).not.toBeInTheDocument(),
    );
    expect(screen.getByText("Grace Hopper")).toBeInTheDocument();
    expect(deleteOrder).toHaveBeenCalledWith(doomed.id);
    finishDelete();
  });

  it("offers no way to reset while demo data is switched off", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(page([summary()]));

    // Act
    renderOrders();
    await screen.findByText("Ada Lovelace");

    // Assert
    expect(
      screen.queryByRole("button", { name: "Reset demo data" }),
    ).not.toBeInTheDocument();
  });

  it("resets only once the dialog is confirmed", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(page([summary()]));
    vi.mocked(getDemoDataStatus).mockResolvedValue({
      enabled: true,
      orderCount: 4,
    });
    renderOrders();
    await screen.findByText("Ada Lovelace");

    // Act
    await userEvent.click(
      screen.getByRole("button", { name: "Reset demo data" }),
    );

    // Assert
    expect(
      screen.getByText(
        "Replace the 4 orders in the database with a fresh demo set? Orders you placed yourself are removed too.",
      ),
    ).toBeInTheDocument();
    expect(resetDemoData).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole("button", { name: "Reset" }));
    await waitFor(() => expect(resetDemoData).toHaveBeenCalledOnce());
  });

  it("leaves the data alone when the reset dialog is cancelled", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(page([summary()]));
    vi.mocked(getDemoDataStatus).mockResolvedValue({
      enabled: true,
      orderCount: 4,
    });
    renderOrders();
    await screen.findByText("Ada Lovelace");
    await userEvent.click(
      screen.getByRole("button", { name: "Reset demo data" }),
    );

    // Act
    await userEvent.click(screen.getAllByRole("button", { name: "Cancel" })[0]);

    // Assert
    expect(resetDemoData).not.toHaveBeenCalled();
  });

  it("keeps the row and explains why when the delete is refused", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(
      page([summary({ customerName: "Ada Lovelace" })]),
    );
    vi.mocked(deleteOrder).mockRejectedValue(
      new ApiError(404, {
        title: "Not found",
        status: 404,
        detail: "No order with that id.",
      }),
    );
    renderOrders();
    await screen.findByText("Ada Lovelace");
    await userEvent.click(screen.getByRole("button", { name: "Delete" }));

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Delete order" }));

    // Assert
    expect(
      await screen.findByText("No order with that id."),
    ).toBeInTheDocument();
    expect(screen.getByText("Ada Lovelace")).toBeInTheDocument();
  });

  it("leaves the order alone when the confirmation is cancelled", async () => {
    // Arrange
    vi.mocked(listOrders).mockResolvedValue(
      page([summary({ customerName: "Ada Lovelace" })]),
    );
    renderOrders();
    await screen.findByText("Ada Lovelace");
    await userEvent.click(screen.getByRole("button", { name: "Delete" }));

    // Act
    await userEvent.click(screen.getAllByRole("button", { name: "Cancel" })[0]);

    // Assert
    expect(deleteOrder).not.toHaveBeenCalled();
    expect(screen.getByText("Ada Lovelace")).toBeInTheDocument();
  });
});
