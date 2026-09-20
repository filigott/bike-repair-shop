import { useTranslation } from "react-i18next";
import { redirect } from "react-router";
import { PlaceOrderForm } from "~/components/orders/place-order-form";
import { actionFailureFrom } from "~/services/action-errors";
import type {
  PlaceOrderRequest,
  ServiceType,
} from "~/services/endpoints/orders";
import { placeOrder } from "~/services/endpoints/orders";
import { listServiceTypes } from "~/services/endpoints/service-types";
import { isActionFailure } from "~/types/action-result";
import type { Route } from "./+types/place-order";

type OrderField = keyof PlaceOrderRequest;

export async function clientLoader() {
  return { serviceTypes: await listServiceTypes() };
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  try {
    await placeOrder(requestFrom(await request.formData()));
  } catch (error) {
    return actionFailureFrom<OrderField>(error);
  }

  return redirect("/");
}

export default function PlaceOrder({
  loaderData,
  actionData,
}: Route.ComponentProps) {
  const { t } = useTranslation();

  return (
    <section className="flex flex-col gap-4">
      <h1 className="text-2xl font-bold">{t("orders.place.title")}</h1>
      <PlaceOrderForm
        serviceTypes={loaderData.serviceTypes}
        failure={
          actionData && isActionFailure(actionData) ? actionData : undefined
        }
      />
    </section>
  );
}

function requestFrom(form: FormData): PlaceOrderRequest {
  const text = (field: string) => String(form.get(field) ?? "").trim();

  return {
    customerName: text("customerName"),
    phoneNumber: text("phoneNumber"),
    email: text("email"),
    bikeBrand: text("bikeBrand"),
    serviceType: text("serviceType") as ServiceType,
    dueDate: text("dueDate"),
    notes: text("notes") || null,
  };
}
