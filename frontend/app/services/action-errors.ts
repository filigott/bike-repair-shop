import { type ActionFailure, ActionResultKind } from "~/types/action-result";
import type { ValidationCode } from "~/types/problems";
import { ApiError } from "./http";

// 4xx becomes a failure the form can render; anything else reaches the error boundary.
export function actionFailureFrom<TField extends string = string>(
  error: unknown,
): ActionFailure<TField> {
  if (error instanceof ApiError && error.status >= 400 && error.status < 500) {
    return {
      kind: ActionResultKind.Failure,
      status: error.status,
      code: error.code,
      fieldErrors: error.fieldErrors as Partial<
        Record<TField, ValidationCode[]>
      >,
    };
  }
  throw error;
}
