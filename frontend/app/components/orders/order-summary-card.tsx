import { useTranslation } from "react-i18next";
import { OrderStatusBadge } from "~/components/orders/order-status-badge";
import { useLocale } from "~/hooks/use-locale";
import type { MaintenanceOrderResponse } from "~/services/endpoints/orders";

export function OrderSummaryCard({
  order,
}: {
  order: MaintenanceOrderResponse;
}) {
  const { t } = useTranslation();
  const { locale } = useLocale();

  return (
    <dl className="grid flex-1 grid-cols-[auto_1fr] content-between gap-x-6 gap-y-4 rounded-box bg-base-200 p-6 text-base">
      <dt className="text-base-content/60">{t("orders.columns.status")}</dt>
      <dd className="flex items-center gap-2 font-medium">
        <OrderStatusBadge status={order.status} />
        {order.isOverdue && (
          <span className="badge badge-error badge-sm">
            {t("orders.overdue")}
          </span>
        )}
      </dd>

      <dt className="text-base-content/60">{t("orders.columns.service")}</dt>
      <dd className="font-medium">
        {t(`orders.serviceType.${order.serviceType}`)}
      </dd>

      <dt className="text-base-content/60">{t("orders.columns.due")}</dt>
      <dd className="font-medium">{formatDay(order.dueDate, locale)}</dd>

      <dt className="text-base-content/60">{t("orders.detail.placedAt")}</dt>
      <dd className="font-medium">{formatDay(order.placedAt, locale)}</dd>
    </dl>
  );
}

function formatDay(value: string, locale: string): string {
  return new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(
    new Date(value.length === 10 ? `${value}T00:00:00` : value),
  );
}
