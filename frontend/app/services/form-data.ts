import type { ServiceType } from "~/services/endpoints/orders";

export function textOf(form: FormData, field: string): string {
  return String(form.get(field) ?? "").trim();
}

export function optionalIntegerOf(
  form: FormData,
  field: string,
): number | null {
  const text = textOf(form, field);
  const value = Number(text);

  return text && Number.isSafeInteger(value) ? value : null;
}

export function optionalTextOf(form: FormData, field: string): string | null {
  return textOf(form, field) || null;
}

const serviceTypes: Record<ServiceType, true> = {
  WheelAdjustment: true,
  ChainReplacement: true,
  BrakeMaintenance: true,
};

export function serviceTypeOf(form: FormData, field: string): ServiceType {
  const value = textOf(form, field);

  if (!isServiceType(value)) {
    throw new Error(`Unknown service type: ${value}`);
  }

  return value;
}

function isServiceType(value: string): value is ServiceType {
  return Object.hasOwn(serviceTypes, value);
}
