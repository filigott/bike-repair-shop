import type { ProblemCode, ValidationCode } from "~/types/problems";

export enum ActionResultKind {
  Success = "success",
  Failure = "failure",
}

export type ActionSuccess<T> = { kind: ActionResultKind.Success; data: T };

export type ActionFailure<TField extends string = string> = {
  kind: ActionResultKind.Failure;
  status: number;
  code: ProblemCode;
  fieldErrors: Partial<Record<TField, ValidationCode[]>>;
};

export type ActionResult<T, TField extends string = string> =
  | ActionSuccess<T>
  | ActionFailure<TField>;

export function actionSuccess<T>(data: T): ActionSuccess<T> {
  return { kind: ActionResultKind.Success, data };
}

export function isActionFailure<T, TField extends string>(
  result: ActionResult<T, TField>,
): result is ActionFailure<TField> {
  return result.kind === ActionResultKind.Failure;
}
