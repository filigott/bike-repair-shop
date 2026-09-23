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
