import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useFetcher } from "react-router";
import { DeleteOrderDialog } from "~/components/orders/delete-order-dialog";
import { OrderSearchForm } from "~/components/orders/order-search-form";
import { OrdersPager } from "~/components/orders/orders-pager";
import { OrdersTable } from "~/components/orders/orders-table";
import { actionFailureFrom } from "~/services/action-errors";
import type { MaintenanceOrderSummary } from "~/services/endpoints/orders";
import { deleteOrder, listOrders } from "~/services/endpoints/orders";
import { textOf } from "~/services/form-data";
import { actionSuccess, isActionFailure } from "~/types/action-result";
import { OrderIntent } from "~/types/intents/orders";
import type { Route } from "./+types/orders";

const PAGE_SIZE = 20;

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const params = new URL(request.url).searchParams;
  const search = params.get("search")?.trim() ?? "";
  const page = Math.max(1, Number(params.get("page") ?? 1) || 1);
  const orders = await listOrders({
    search: search || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  return { orders, search, page };
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const form = await request.formData();
  const intent = textOf(form, "intent");
  if (intent !== OrderIntent.Delete) {
    throw new Error(`Unknown order intent: ${intent}`);
  }

  try {
    await deleteOrder(textOf(form, "id"));
  } catch (error) {
    return actionFailureFrom(error);
  }

  return actionSuccess(null);
}

export default function Orders({ loaderData }: Route.ComponentProps) {
  const { orders, search, page } = loaderData;
  const { t } = useTranslation();
  const fetcher = useFetcher();
  const [selected, setSelected] = useState<MaintenanceOrderSummary | null>(
    null,
  );

  const deletingId = fetcher.formData?.get("id");
  const items = orders.items.filter((order) => order.id !== deletingId);
  const failure =
    fetcher.data && isActionFailure(fetcher.data) ? fetcher.data : undefined;

  return (
    <section className="flex flex-col gap-4">
      <div className="flex items-center gap-4">
        <h1 className="text-2xl font-bold">{t("orders.title")}</h1>
        <Link to="/orders/place" className="btn btn-primary btn-sm ml-auto">
          {t("orders.place.action")}
        </Link>
      </div>
      <OrderSearchForm search={search} />
      {failure && (
        <p role="alert" className="alert alert-error">
          {failure.detail ?? failure.title}
        </p>
      )}
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
    </section>
  );
}
