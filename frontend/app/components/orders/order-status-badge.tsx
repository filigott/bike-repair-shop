import { useTranslation } from "react-i18next";
import type { OrderStatus } from "~/services/endpoints/orders";

const badgeClasses: Record<OrderStatus, string> = {
  Received: "badge-info",
  InProgress: "badge-warning",
  Completed: "badge-success",
  Cancelled: "badge-error",
};

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  const { t } = useTranslation();

  return (
    <span className={`badge badge-soft ${badgeClasses[status]}`}>
      {t(`orders.status.${status}`)}
    </span>
  );
}
