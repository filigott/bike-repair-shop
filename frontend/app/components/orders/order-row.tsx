import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { OrderStatusBadge } from "~/components/orders/order-status-badge";
import { useLocale } from "~/hooks/use-locale";
import type { MaintenanceOrderSummary } from "~/services/endpoints/orders";

export function OrderRow({
  order,
  onDelete,
}: {
  order: MaintenanceOrderSummary;
  onDelete: (order: MaintenanceOrderSummary) => void;
}) {
  const { t } = useTranslation();
  const { locale } = useLocale();

  return (
    <tr>
      <td className="font-mono">
        {t("orders.reference", { number: order.orderNumber })}
      </td>
      <td>{order.customerName}</td>
      <td>{order.phoneNumber}</td>
      <td>{order.bikeBrand}</td>
      <td>{t(`orders.serviceType.${order.serviceType}`)}</td>
      <td className="flex items-center gap-2">
        {formatDueDate(order.dueDate, locale)}
        {order.isOverdue && (
          <span className="badge badge-error badge-sm">
            {t("orders.overdue")}
          </span>
        )}
      </td>
      <td>
        <OrderStatusBadge status={order.status} />
      </td>
      <td className="flex gap-1">
        <Link
          to={`/orders/${order.id}`}
          className="btn btn-primary btn-soft btn-xs"
        >
          {t("orders.detail.action")}
        </Link>
        <button
          type="button"
          className="btn btn-error btn-soft btn-xs"
          onClick={() => onDelete(order)}
        >
          {t("orders.delete.action")}
        </button>
      </td>
    </tr>
  );
}

// The API sends a calendar day, so parse it as local midnight rather than UTC.
function formatDueDate(dueDate: string, locale: string): string {
  return new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(
    new Date(`${dueDate}T00:00:00`),
  );
}
