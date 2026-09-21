import { useTranslation } from "react-i18next";
import type { OrderStatus } from "~/services/endpoints/orders";

// Typed per status, so a new status fails the build instead of falling through to a default colour.
const badgeClasses: Record<OrderStatus, string> = {
  Received: "badge-info",
  InProgress: "badge-warning",
  Completed: "badge-success",
  Cancelled: "badge-neutral",
};

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  const { t } = useTranslation();

  return (
    <span className={`badge badge-soft ${badgeClasses[status]}`}>
      {t(`orders.status.${status}`)}
    </span>
  );
}
