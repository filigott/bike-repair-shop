export function textOf(form: FormData, field: string): string {
  return String(form.get(field) ?? "").trim();
}

export function optionalTextOf(form: FormData, field: string): string | null {
  return textOf(form, field) || null;
}
