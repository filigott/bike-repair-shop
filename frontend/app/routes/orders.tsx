import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { OrderSearchForm } from "~/components/orders/order-search-form";
import { OrdersPager } from "~/components/orders/orders-pager";
import { OrdersTable } from "~/components/orders/orders-table";
import { listOrders } from "~/services/endpoints/orders";
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

export default function Orders({ loaderData }: Route.ComponentProps) {
  const { orders, search, page } = loaderData;
  const { t } = useTranslation();

  return (
    <section className="flex flex-col gap-4">
      <div className="flex items-center gap-4">
        <h1 className="text-2xl font-bold">{t("orders.title")}</h1>
        <Link to="/orders/place" className="btn btn-primary btn-sm ml-auto">
          {t("orders.place.action")}
        </Link>
      </div>
      <OrderSearchForm search={search} />
      {orders.items.length === 0 ? (
        <p className="text-base-content/70">{t("orders.empty")}</p>
      ) : (
        <>
          <OrdersTable items={orders.items} />
          <OrdersPager
            search={search}
            page={page}
            pageSize={Number(orders.pageSize)}
            totalCount={Number(orders.totalCount)}
          />
        </>
      )}
    </section>
  );
}
