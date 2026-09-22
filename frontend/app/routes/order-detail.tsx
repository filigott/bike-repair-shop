import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { CommandForm } from "~/components/orders/command-form";
import { OrderContactCard } from "~/components/orders/order-contact-card";
import {
  DueDateField,
  OrderDetailsFields,
  type OrderFormField,
  ServiceTypeField,
} from "~/components/orders/order-fields";
import { OrderSummaryCard } from "~/components/orders/order-summary-card";
import { actionFailureFrom } from "~/services/action-errors";
import type { ServiceType } from "~/services/endpoints/orders";
import {
  changeOrderService,
  correctOrderDetails,
  getOrder,
  rescheduleOrder,
} from "~/services/endpoints/orders";
import { listServiceTypes } from "~/services/endpoints/service-types";
import { optionalTextOf, textOf } from "~/services/form-data";
import { actionSuccess } from "~/types/action-result";
import { OrderIntent } from "~/types/intents/orders";
import type { Route } from "./+types/order-detail";

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const [order, serviceTypes] = await Promise.all([
    getOrder(params.id),
    listServiceTypes(),
  ]);

  return { order, serviceTypes };
}

export async function clientAction({
  params,
  request,
}: Route.ClientActionArgs) {
  const form = await request.formData();

  try {
    await runIntent(params.id, form);
  } catch (error) {
    return actionFailureFrom<OrderFormField>(error);
  }

  return actionSuccess(null);
}

export default function OrderDetail({ loaderData }: Route.ComponentProps) {
  const { order, serviceTypes } = loaderData;
  const { t } = useTranslation();

  return (
    <section className="flex max-w-xl flex-col gap-4">
      <div className="flex items-center gap-4">
        <div>
          <h1 className="font-mono text-2xl font-bold">
            {t("orders.reference", { number: order.orderNumber })}
          </h1>
          <p className="text-base-content/70">{order.customerName}</p>
        </div>
        <Link to="/" className="btn btn-ghost btn-sm ml-auto">
          {t("common.back")}
        </Link>
      </div>

      <OrderSummaryCard order={order} />

      {order.isEditable ? (
        <>
          <CommandForm
            intent={OrderIntent.CorrectDetails}
            legend={t("orders.correctDetails.legend")}
            submitLabel={t("orders.correctDetails.submit")}
          >
            {(errors) => (
              <OrderDetailsFields defaults={order} errors={errors} />
            )}
          </CommandForm>

          <CommandForm
            intent={OrderIntent.ChangeService}
            legend={t("orders.changeService.legend")}
            submitLabel={t("orders.changeService.submit")}
          >
            {(errors) => (
              <ServiceTypeField
                serviceTypes={serviceTypes}
                defaults={order}
                errors={errors}
              />
            )}
          </CommandForm>

          <CommandForm
            intent={OrderIntent.Reschedule}
            legend={t("orders.reschedule.legend")}
            submitLabel={t("orders.reschedule.submit")}
          >
            {(errors) => <DueDateField defaults={order} errors={errors} />}
          </CommandForm>
        </>
      ) : (
        <>
          <p role="alert" className="alert alert-warning">
            {t("orders.detail.closed", {
              status: t(`orders.status.${order.status}`),
            })}
          </p>
          <OrderContactCard order={order} />
        </>
      )}
    </section>
  );
}

function runIntent(id: string, form: FormData): Promise<unknown> {
  switch (textOf(form, "intent")) {
    case OrderIntent.CorrectDetails:
      return correctOrderDetails(id, {
        customerName: textOf(form, "customerName"),
        phoneNumber: textOf(form, "phoneNumber"),
        email: textOf(form, "email"),
        bikeBrand: textOf(form, "bikeBrand"),
        notes: optionalTextOf(form, "notes"),
      });
    case OrderIntent.ChangeService:
      return changeOrderService(id, {
        serviceType: textOf(form, "serviceType") as ServiceType,
      });
    case OrderIntent.Reschedule:
      return rescheduleOrder(id, { dueDate: optionalTextOf(form, "dueDate") });
    default:
      throw new Error(`Unknown order intent: ${textOf(form, "intent")}`);
  }
}
