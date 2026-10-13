import { useTranslation } from "react-i18next";
import { redirect } from "react-router";
import type { OrderFormField } from "~/components/orders/order-fields";
import { OrderForm } from "~/components/orders/order-form";
import { actionFailureFrom } from "~/services/action-errors";
import type { PlaceOrderRequest } from "~/services/endpoints/orders";
import { placeOrder } from "~/services/endpoints/orders";
import { listServiceTypes } from "~/services/endpoints/service-types";
import { optionalTextOf, serviceTypeOf, textOf } from "~/services/form-data";
import { isActionFailure } from "~/types/action-result";
import type { Route } from "./+types/place-order";

export async function clientLoader() {
  return { serviceTypes: await listServiceTypes() };
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  try {
    await placeOrder(requestFrom(await request.formData()));
  } catch (error) {
    return actionFailureFrom<OrderFormField>(error);
  }

  return redirect("/");
}

export default function PlaceOrder({
  loaderData,
  actionData,
}: Route.ComponentProps) {
  const { t } = useTranslation();

  return (
    <section className="mx-auto flex w-full max-w-2xl flex-col gap-6">
      <h1 className="text-2xl font-bold">{t("orders.place.title")}</h1>
      <OrderForm
        serviceTypes={loaderData.serviceTypes}
        submitLabel={t("orders.place.submit")}
        failure={
          actionData && isActionFailure(actionData) ? actionData : undefined
        }
      />
    </section>
  );
}

function requestFrom(form: FormData): PlaceOrderRequest {
  return {
    customerName: textOf(form, "customerName"),
    phoneNumber: textOf(form, "phoneNumber"),
    email: textOf(form, "email"),
    bikeBrand: textOf(form, "bikeBrand"),
    serviceType: serviceTypeOf(form, "serviceType"),
    dueDate: optionalTextOf(form, "dueDate"),
    notes: optionalTextOf(form, "notes"),
  };
}
