import createClient from "openapi-fetch";
import {
  type ProblemCode,
  problemCodeOf,
  type ValidationCode,
  validationCodesOf,
} from "~/types/problems";
import type { components, paths } from "./openapi.generated";

export type ProblemDetails = components["schemas"]["ProblemDetails"];
export type ValidationProblemDetails =
  components["schemas"]["ValidationProblemDetails"];

// The API adds these as ProblemDetails extensions, which the OpenAPI schema does not describe.
type ProblemExtensions = { code?: unknown; errorCodes?: unknown };

export type Problem = (ProblemDetails | ValidationProblemDetails) &
  ProblemExtensions;

type FetchResult<T> = {
  data?: T;
  error?: unknown;
  response: Response;
};

const REQUEST_TIMEOUT_MS = 10_000;

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly problem: Problem,
  ) {
    super(problem.title ?? `Request failed with status ${status}`);
    this.name = "ApiError";
  }

  get code(): ProblemCode {
    return problemCodeOf(this.problem.code);
  }

  get fieldErrors(): Record<string, ValidationCode[]> {
    return validationCodesOf(this.problem.errorCodes);
  }
}

// The client builds an absolute Request, so the same-origin base has to be explicit.
const baseUrl =
  typeof window === "undefined" ? "http://localhost" : window.location.origin;

export const apiClient = createClient<paths>({
  baseUrl,
  headers: { accept: "application/json" },
  // Resolved per call so each request gets its own deadline, and so tests can stub fetch.
  fetch: (request) =>
    fetch(request, { signal: AbortSignal.timeout(REQUEST_TIMEOUT_MS) }),
});

export async function unwrap<T>(call: Promise<FetchResult<T>>): Promise<T> {
  const { data, error, response } = await call;
  if (!response.ok) {
    throw new ApiError(response.status, toProblem(error, response));
  }
  return data as T;
}

function toProblem(error: unknown, response: Response): Problem {
  return error && typeof error === "object"
    ? (error as Problem)
    : { status: response.status, title: response.statusText };
}
