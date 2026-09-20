export enum ActionResultKind {
  Success = "success",
  Failure = "failure",
}

export type ActionSuccess<T> = { kind: ActionResultKind.Success; data: T };

export type ActionFailure = {
  kind: ActionResultKind.Failure;
  status: number;
  title: string;
  detail?: string;
  fieldErrors: Record<string, string[]>;
};

export type ActionResult<T> = ActionSuccess<T> | ActionFailure;

export function actionSuccess<T>(data: T): ActionSuccess<T> {
  return { kind: ActionResultKind.Success, data };
}

export function isActionFailure<T>(
  result: ActionResult<T>,
): result is ActionFailure {
  return result.kind === ActionResultKind.Failure;
}
