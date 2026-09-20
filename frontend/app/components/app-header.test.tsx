import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createRoutesStub } from "react-router";
import { describe, expect, it } from "vitest";
import { AppHeader } from "./app-header";

describe("AppHeader", () => {
  it("shows the title and switches locale", async () => {
    // Arrange
    const Stub = createRoutesStub([{ path: "/", Component: AppHeader }]);
    render(<Stub />);
    expect(
      screen.getByRole("link", { name: "Bike Repair Shop" }),
    ).toBeInTheDocument();

    // Act
    await userEvent.selectOptions(
      screen.getByRole("combobox", { name: "Language" }),
      "nb",
    );

    // Assert
    expect(
      screen.getByRole("link", { name: "Sykkelverkstedet" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("combobox", { name: "Språk" })).toHaveValue("nb");
  });
});
