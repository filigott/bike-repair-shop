import { type ActionFailure, ActionResultKind } from "~/types/action-result";
import { ApiError } from "./http";

// 4xx becomes a failure the form can render; anything else reaches the error boundary.
export function actionFailureFrom(error: unknown): ActionFailure {
  if (error instanceof ApiError && error.status >= 400 && error.status < 500) {
    return {
      kind: ActionResultKind.Failure,
      status: error.status,
      title: error.problem.title ?? error.message,
      detail: error.problem.detail ?? undefined,
      fieldErrors: error.fieldErrors,
    };
  }
  throw error;
}
