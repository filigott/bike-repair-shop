import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import Orders from "./orders";

describe("Orders", () => {
  it("renders the page heading", () => {
    // Arrange & Act
    render(<Orders />);

    // Assert
    expect(screen.getByRole("heading", { name: "Orders" })).toBeInTheDocument();
  });
});
