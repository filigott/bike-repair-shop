import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createRoutesStub } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type {
  MaintenanceOrderSummary,
  PagedOrders,
} from "~/services/endpoints/orders";
import { listOrders } from "~/services/endpoints/orders";
import Orders, { clientLoader } from "./orders";

vi.mock("~/services/endpoints/orders", () => ({ listOrders: vi.fn() }));

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
    },
  ]);
  return render(<Stub initialEntries={[url]} />);
}

describe("Orders", () => {
  beforeEach(() => vi.mocked(listOrders).mockReset());

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
});
