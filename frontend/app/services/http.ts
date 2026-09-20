import type { components } from "./openapi.generated";

export type ProblemDetails = components["schemas"]["ProblemDetails"];
export type ValidationProblemDetails =
  components["schemas"]["ValidationProblemDetails"];

const REQUEST_TIMEOUT_MS = 10_000;

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly problem: ProblemDetails,
  ) {
    super(problem.title ?? `Request failed with status ${status}`);
    this.name = "ApiError";
  }

  get fieldErrors(): Record<string, string[]> {
    return (this.problem as ValidationProblemDetails).errors ?? {};
  }
}

export async function request<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: {
      accept: "application/json",
      ...(init.body ? { "content-type": "application/json" } : {}),
      ...init.headers,
    },
    signal: init.signal ?? AbortSignal.timeout(REQUEST_TIMEOUT_MS),
  });

  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response));
  }

  return response.status === 204
    ? (undefined as T)
    : ((await response.json()) as T);
}

export function toQueryString(
  params: Record<string, string | number | undefined>,
): string {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") {
      query.set(key, String(value));
    }
  }
  const text = query.toString();
  return text ? `?${text}` : "";
}

async function readProblem(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return { status: response.status, title: response.statusText };
  }
}
