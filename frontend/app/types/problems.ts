export enum ProblemCode {
  InvalidRequest = "InvalidRequest",
  OrderNotFound = "OrderNotFound",
  OrderChanged = "OrderChanged",
  DueDateInPast = "DueDateInPast",
  OrderNotEditable = "OrderNotEditable",
  InvalidTransition = "InvalidTransition",
  Unknown = "Unknown",
}

export enum ValidationCode {
  Invalid = "Invalid",
  Required = "Required",
  TooLong = "TooLong",
  InvalidPhoneNumber = "InvalidPhoneNumber",
  InvalidEmail = "InvalidEmail",
  UnknownValue = "UnknownValue",
}

const problemCodes = Object.values(ProblemCode);
const validationCodes = Object.values(ValidationCode);

export function problemCodeOf(value: unknown): ProblemCode {
  return problemCodes.find((code) => code === value) ?? ProblemCode.Unknown;
}

export function validationCodeOf(value: unknown): ValidationCode {
  return (
    validationCodes.find((code) => code === value) ?? ValidationCode.Invalid
  );
}

export function validationCodesOf(
  value: unknown,
): Record<string, ValidationCode[]> {
  if (!value || typeof value !== "object") {
    return {};
  }

  return Object.fromEntries(
    Object.entries(value).map(([field, codes]) => [
      field,
      Array.isArray(codes) ? codes.map(validationCodeOf) : [],
    ]),
  );
}
