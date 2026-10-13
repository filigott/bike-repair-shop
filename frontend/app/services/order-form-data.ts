import type { CorrectOrderDetailsRequest } from "~/services/endpoints/orders";
import { optionalTextOf, textOf } from "~/services/form-data";

export type OrderDetails = Omit<CorrectOrderDetailsRequest, "version">;

export function orderDetailsFrom(form: FormData): OrderDetails {
  return {
    customerName: textOf(form, "customerName"),
    phoneNumber: textOf(form, "phoneNumber"),
    email: textOf(form, "email"),
    bikeBrand: textOf(form, "bikeBrand"),
    notes: optionalTextOf(form, "notes"),
  };
}
