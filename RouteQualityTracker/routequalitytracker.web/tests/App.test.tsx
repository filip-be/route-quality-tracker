import App from "../src/App";
import "@testing-library/jest-dom";
import { render } from "@testing-library/react";
import { describe, it, expect } from "vitest";

describe("<App />", () => {
  it("Renders title", () => {
    const component = render(<App />);

    expect(component.getByText("Vite + React")).toBeInTheDocument();
  });
});
