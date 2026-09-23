import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createRoutesStub } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { placeOrder } from "~/services/endpoints/orders";
import { listServiceTypes } from "~/services/endpoints/service-types";
import { ApiError } from "~/services/http";
import PlaceOrder, { clientAction, clientLoader } from "./place-order";

vi.mock("~/services/endpoints/orders", () => ({ placeOrder: vi.fn() }));
vi.mock("~/services/endpoints/service-types", () => ({
  listServiceTypes: vi.fn(),
}));

function renderForm() {
  const Stub = createRoutesStub([
    {
      path: "/orders/place",
      Component: PlaceOrder,
      loader: () => clientLoader(),
      action: (args) =>
        clientAction(args as Parameters<typeof clientAction>[0]),
    },
    { path: "/", Component: () => <h1>Orders</h1> },
  ]);
  return render(<Stub initialEntries={["/orders/place"]} />);
}

async function fillTheForm() {
  await userEvent.type(
    await screen.findByLabelText("Customer name"),
    "Ada Lovelace",
  );
  await userEvent.type(screen.getByLabelText("Phone number"), "+4712345678");
  await userEvent.type(screen.getByLabelText("Email"), "ada@example.com");
  await userEvent.type(screen.getByLabelText("Bike brand"), "Bianchi");
  await userEvent.selectOptions(
    screen.getByLabelText("Service"),
    "ChainReplacement",
  );
  await userEvent.type(screen.getByLabelText("Due date"), "2026-09-25");
}

describe("PlaceOrder", () => {
  beforeEach(() => {
    vi.mocked(placeOrder).mockReset();
    vi.mocked(listServiceTypes).mockResolvedValue([
      { code: "WheelAdjustment", displayName: "Wheel adjustment" },
      { code: "ChainReplacement", displayName: "Chain replacement" },
    ]);
  });

  it("sends the filled form as a place-order command", async () => {
    // Arrange
    renderForm();
    await fillTheForm();

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Place order" }));

    // Assert
    await waitFor(() =>
      expect(placeOrder).toHaveBeenCalledWith({
        customerName: "Ada Lovelace",
        phoneNumber: "+4712345678",
        email: "ada@example.com",
        bikeBrand: "Bianchi",
        serviceType: "ChainReplacement",
        dueDate: "2026-09-25",
        notes: null,
      }),
    );
  });

  it("leaves a missing due date to the API rather than blocking the submit", async () => {
    // Arrange
    renderForm();
    await userEvent.type(
      await screen.findByLabelText("Customer name"),
      "Ada Lovelace",
    );
    await userEvent.type(screen.getByLabelText("Phone number"), "+4712345678");
    await userEvent.type(screen.getByLabelText("Email"), "ada@example.com");
    await userEvent.type(screen.getByLabelText("Bike brand"), "Bianchi");

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Place order" }));

    // Assert
    await waitFor(() =>
      expect(placeOrder).toHaveBeenCalledWith(
        expect.objectContaining({ dueDate: null }),
      ),
    );
  });

  it("returns to the list once the order is placed", async () => {
    // Arrange
    vi.mocked(placeOrder).mockResolvedValue({} as never);
    renderForm();
    await fillTheForm();

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Place order" }));

    // Assert
    expect(
      await screen.findByRole("heading", { name: "Orders" }),
    ).toBeInTheDocument();
  });

  it("renders the rejected fields next to their inputs", async () => {
    // Arrange
    vi.mocked(placeOrder).mockRejectedValue(
      new ApiError(400, {
        title: "Validation failed",
        status: 400,
        errors: { email: ["'Email' must be a valid email address."] },
        errorCodes: { email: ["InvalidEmail"] },
        code: "InvalidRequest",
      }),
    );
    renderForm();
    await fillTheForm();

    // Act
    await userEvent.click(screen.getByRole("button", { name: "Place order" }));

    // Assert
    expect(
      await screen.findByText("Enter a valid email address."),
    ).toBeInTheDocument();
    expect(screen.getByText("Some fields need attention.")).toBeInTheDocument();
    expect(
      screen.queryByText("'Email' must be a valid email address."),
    ).not.toBeInTheDocument();
  });
});
