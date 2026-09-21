import { useTranslation } from "react-i18next";
import type { MaintenanceOrderSummary } from "~/services/endpoints/orders";
import { OrderRow } from "./order-row";

export function OrdersTable({ items }: { items: MaintenanceOrderSummary[] }) {
  const { t } = useTranslation();

  return (
    <table className="table">
      <thead>
        <tr>
          <th>{t("orders.columns.customer")}</th>
          <th>{t("orders.columns.phone")}</th>
          <th>{t("orders.columns.bike")}</th>
          <th>{t("orders.columns.service")}</th>
          <th>{t("orders.columns.due")}</th>
          <th>{t("orders.columns.status")}</th>
          <th>{t("common.actions")}</th>
        </tr>
      </thead>
      <tbody>
        {items.map((order) => (
          <OrderRow key={order.id} order={order} />
        ))}
      </tbody>
    </table>
  );
}
