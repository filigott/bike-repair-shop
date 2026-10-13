// The API sends a calendar day, so parse it as local midnight rather than UTC.
export function formatCalendarDay(day: string, locale: string): string {
  return dayFormat(locale).format(new Date(`${day}T00:00:00`));
}

export function formatInstant(instant: string, locale: string): string {
  return dayFormat(locale).format(new Date(instant));
}

function dayFormat(locale: string): Intl.DateTimeFormat {
  return new Intl.DateTimeFormat(locale, { dateStyle: "medium" });
}
