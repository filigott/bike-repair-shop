import {
  isRouteErrorResponse,
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
} from "react-router";

import type { Route } from "./+types/root";
import "./app.css";

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <Meta />
        <Links />
      </head>
      <body>
        {children}
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export default function App() {
  return (
    <main className="container mx-auto p-4">
      <Outlet />
    </main>
  );
}

export function ErrorBoundary({ error }: Route.ErrorBoundaryProps) {
  const message = isRouteErrorResponse(error)
    ? `${error.status} ${error.statusText}`
    : "Something went wrong";
  const details =
    import.meta.env.DEV && error instanceof Error ? error.stack : undefined;

  return (
    <main className="container mx-auto p-4">
      <h1 className="text-2xl font-bold">{message}</h1>
      {details && (
        <pre className="mt-4 overflow-x-auto p-4">
          <code>{details}</code>
        </pre>
      )}
    </main>
  );
}
