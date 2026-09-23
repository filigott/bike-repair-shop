import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useFetcher } from "react-router";
import { ResetDemoDataDialog } from "~/components/demo-data/reset-demo-data-dialog";
import { DeleteOrderDialog } from "~/components/orders/delete-order-dialog";
import { OrderSearchForm } from "~/components/orders/order-search-form";
import { OrderStatusLegend } from "~/components/orders/order-status-legend";
import { OrdersPager } from "~/components/orders/orders-pager";
import { OrdersTable } from "~/components/orders/orders-table";
import { ProblemAlert } from "~/components/problem-alert";
import { actionFailureFrom } from "~/services/action-errors";
import {
  getDemoDataStatus,
  resetDemoData,
} from "~/services/endpoints/demo-data";
import { listOrderStatuses } from "~/services/endpoints/order-statuses";
import type { MaintenanceOrderSummary } from "~/services/endpoints/orders";
import { deleteOrder, listOrders } from "~/services/endpoints/orders";
import { textOf } from "~/services/form-data";
import { actionSuccess, isActionFailure } from "~/types/action-result";
import { DemoDataIntent } from "~/types/intents/demo-data";
import { OrderIntent } from "~/types/intents/orders";
import type { Route } from "./+types/orders";

const PAGE_SIZE = 20;

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const params = new URL(request.url).searchParams;
  const search = params.get("search")?.trim() ?? "";
  const page = Math.max(1, Number(params.get("page") ?? 1) || 1);
  const [orders, demoData, statuses] = await Promise.all([
    listOrders({ search: search || undefined, page, pageSize: PAGE_SIZE }),
    getDemoDataStatus(),
    listOrderStatuses(),
  ]);

  return { orders, demoData, statuses, search, page };
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const form = await request.formData();

  try {
    await runIntent(form);
  } catch (error) {
    return actionFailureFrom(error);
  }

  return actionSuccess(null);
}

export default function Orders({ loaderData }: Route.ComponentProps) {
  const { orders, demoData, statuses, search, page } = loaderData;
  const { t } = useTranslation();
  const fetcher = useFetcher();
  const [selected, setSelected] = useState<MaintenanceOrderSummary | null>(
    null,
  );
  const [resetting, setResetting] = useState(false);

  const deletingId = fetcher.formData?.get("id");
  const items = orders.items.filter((order) => order.id !== deletingId);
  const failure =
    fetcher.data && isActionFailure(fetcher.data) ? fetcher.data : undefined;

  return (
    <section className="flex flex-col gap-4">
      <div className="flex items-center gap-4">
        <h1 className="text-2xl font-bold">{t("orders.title")}</h1>
        <div className="ml-auto flex items-center gap-2">
          {demoData.enabled && (
            <button
              type="button"
              className="btn btn-warning btn-soft btn-sm"
              onClick={() => setResetting(true)}
            >
              {t("demoData.action")}
            </button>
          )}
          <Link to="/orders/place" className="btn btn-primary btn-sm">
            {t("orders.place.action")}
          </Link>
        </div>
      </div>
      <OrderStatusLegend statuses={statuses} />
      <OrderSearchForm search={search} />
      {failure && <ProblemAlert failure={failure} />}
      {items.length === 0 ? (
        <p className="text-base-content/70">{t("orders.empty")}</p>
      ) : (
        <>
          <OrdersTable items={items} onDelete={setSelected} />
          <OrdersPager
            search={search}
            page={page}
            pageSize={Number(orders.pageSize)}
            totalCount={Number(orders.totalCount)}
          />
        </>
      )}
      {selected && (
        <DeleteOrderDialog
          order={selected}
          onClose={() => setSelected(null)}
          onConfirm={() => {
            fetcher.submit(
              { intent: OrderIntent.Delete, id: selected.id },
              { method: "post" },
            );
            setSelected(null);
          }}
        />
      )}
      {resetting && (
        <ResetDemoDataDialog
          orderCount={Number(demoData.orderCount)}
          onClose={() => setResetting(false)}
          onConfirm={() => {
            fetcher.submit(
              { intent: DemoDataIntent.Reset },
              { method: "post" },
            );
            setResetting(false);
          }}
        />
      )}
    </section>
  );
}

function runIntent(form: FormData): Promise<unknown> {
  const intent = textOf(form, "intent");
  switch (intent) {
    case OrderIntent.Delete:
      return deleteOrder(textOf(form, "id"));
    case DemoDataIntent.Reset:
      return resetDemoData();
    default:
      throw new Error(`Unknown intent: ${intent}`);
  }
}
