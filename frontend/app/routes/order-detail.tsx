import { useState } from "react";
import { useTranslation } from "react-i18next";
import {
  Link,
  redirect,
  type ShouldRevalidateFunctionArgs,
  useFetcher,
  useLocation,
  useNavigate,
  useSearchParams,
} from "react-router";
import { CommandDialog } from "~/components/orders/command-dialog";
import { ConfirmDialog } from "~/components/orders/confirm-dialog";
import { DeleteOrderDialog } from "~/components/orders/delete-order-dialog";
import { OrderActions, toneOf } from "~/components/orders/order-actions";
import { OrderContactCard } from "~/components/orders/order-contact-card";
import {
  DueDateField,
  OrderDetailsFields,
  type OrderFormField,
  ServiceTypeField,
} from "~/components/orders/order-fields";
import { OrderSummaryCard } from "~/components/orders/order-summary-card";
import { actionFailureFrom } from "~/services/action-errors";
import type {
  MaintenanceOrderResponse,
  OrderAction,
  ServiceType,
} from "~/services/endpoints/orders";
import {
  cancelOrder,
  changeOrderService,
  completeOrder,
  correctOrderDetails,
  deleteOrder,
  getOrder,
  reopenOrder,
  rescheduleOrder,
  startOrder,
} from "~/services/endpoints/orders";
import { listServiceTypes } from "~/services/endpoints/service-types";
import { optionalTextOf, textOf } from "~/services/form-data";
import { actionSuccess, isActionFailure } from "~/types/action-result";
import { editCommands, OrderIntent } from "~/types/intents/orders";
import {
  EDIT_PARAM,
  editCommandOf,
  isEditCommand,
} from "~/types/params/orders";
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
  const intent = textOf(form, "intent") as OrderIntent;

  try {
    await runIntent(params.id, intent, form);
  } catch (error) {
    return actionFailureFrom<OrderFormField>(error);
  }

  if (intent === OrderIntent.Delete) {
    return redirect("/");
  }

  // An accepted edit closes its dialog by navigating off the param that opened it.
  return isEditCommand(intent)
    ? redirect(new URL(request.url).pathname)
    : actionSuccess(null);
}

export function shouldRevalidate({
  formMethod,
  currentUrl,
  nextUrl,
  defaultShouldRevalidate,
}: ShouldRevalidateFunctionArgs) {
  const dialogOnly =
    formMethod === undefined && currentUrl.pathname === nextUrl.pathname;

  return dialogOnly ? false : defaultShouldRevalidate;
}

export default function OrderDetail({ loaderData }: Route.ComponentProps) {
  const { order, serviceTypes } = loaderData;
  const { t } = useTranslation();
  const fetcher = useFetcher();
  const [pending, setPending] = useState<OrderAction | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const editing = order.isEditable
    ? editCommandOf(searchParams.get(EDIT_PARAM))
    : null;
  const closeDialog = () => navigate(pathname, { replace: true });
  const failure =
    fetcher.data && isActionFailure(fetcher.data) ? fetcher.data : undefined;

  return (
    <section className="mx-auto flex w-full max-w-4xl flex-col gap-6">
      <Link to="/" className="btn btn-outline btn-sm w-fit">
        {t("orders.backToList")}
      </Link>

      <header>
        <h1 className="font-mono text-4xl font-bold">
          {t("orders.reference", { number: order.orderNumber })}
        </h1>
        <p className="text-base-content/70 text-lg">{order.customerName}</p>
      </header>

      <div className="grid gap-6 md:grid-cols-[1fr_16rem]">
        <div className="flex flex-col gap-6">
          <OrderSummaryCard order={order} />
          <OrderContactCard order={order} />
        </div>

        <div className="flex flex-col gap-4 self-start rounded-box border border-base-300 p-5">
          <h2 className="font-semibold text-base-content/60 text-sm uppercase tracking-wide">
            {t("common.actions")}
          </h2>

          {failure && (
            <p role="alert" className="alert alert-error">
              {failure.detail ?? failure.title}
            </p>
          )}

          {!order.isEditable && (
            <p role="alert" className="alert alert-warning">
              {t("orders.detail.closed", {
                status: t(`orders.status.${order.status}`),
              })}
            </p>
          )}

          <OrderActions
            actions={order.allowedActions}
            edits={order.isEditable ? editCommands : []}
            onAction={setPending}
            onDelete={() => setDeleting(true)}
          />
        </div>
      </div>

      {editing === OrderIntent.CorrectDetails && (
        <CommandDialog
          intent={OrderIntent.CorrectDetails}
          title={t("orders.correctDetails.action")}
          submitLabel={t("common.save")}
          onClose={closeDialog}
        >
          {(errors) => <OrderDetailsFields defaults={order} errors={errors} />}
        </CommandDialog>
      )}

      {editing === OrderIntent.ChangeService && (
        <CommandDialog
          intent={OrderIntent.ChangeService}
          title={t("orders.changeService.action")}
          submitLabel={t("common.save")}
          onClose={closeDialog}
        >
          {(errors) => (
            <ServiceTypeField
              serviceTypes={serviceTypes}
              defaults={order}
              errors={errors}
            />
          )}
        </CommandDialog>
      )}

      {editing === OrderIntent.Reschedule && (
        <CommandDialog
          intent={OrderIntent.Reschedule}
          title={t("orders.reschedule.action")}
          submitLabel={t("common.save")}
          onClose={closeDialog}
        >
          {(errors) => <DueDateField defaults={order} errors={errors} />}
        </CommandDialog>
      )}

      {deleting && (
        <DeleteOrderDialog
          order={order}
          onClose={() => setDeleting(false)}
          onConfirm={() => {
            fetcher.submit({ intent: OrderIntent.Delete }, { method: "post" });
            setDeleting(false);
          }}
        />
      )}

      {pending && (
        <ConfirmDialog
          title={t(`orders.actions.${pending}`)}
          message={t(`orders.confirm.${pending}`, {
            reference: t("orders.reference", { number: order.orderNumber }),
            customer: order.customerName,
          })}
          confirmLabel={t(`orders.actions.${pending}`)}
          tone={toneOf(pending)}
          onClose={() => setPending(null)}
          onConfirm={() => {
            fetcher.submit(
              { intent: OrderIntent.Transition, action: pending },
              { method: "post" },
            );
            setPending(null);
          }}
        />
      )}
    </section>
  );
}

const transitions: Record<
  OrderAction,
  (id: string) => Promise<MaintenanceOrderResponse>
> = {
  Start: startOrder,
  Complete: completeOrder,
  Cancel: cancelOrder,
  Reopen: reopenOrder,
};

function runIntent(
  id: string,
  intent: OrderIntent,
  form: FormData,
): Promise<unknown> {
  switch (intent) {
    case OrderIntent.Transition:
      return transitions[textOf(form, "action") as OrderAction](id);
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
    case OrderIntent.Delete:
      return deleteOrder(id);
    default:
      throw new Error(`Unknown order intent: ${intent}`);
  }
}
