import { useTranslation } from "react-i18next";
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
    <dl className="grid grid-cols-2 gap-2 rounded-box bg-base-200 p-4 text-sm">
      <dt className="font-semibold">{t("orders.columns.status")}</dt>
      <dd className="flex items-center gap-2">
        <span className="badge badge-ghost">
          {t(`orders.status.${order.status}`)}
        </span>
        {order.isOverdue && (
          <span className="badge badge-error badge-sm">
            {t("orders.overdue")}
          </span>
        )}
      </dd>

      <dt className="font-semibold">{t("orders.columns.service")}</dt>
      <dd>{t(`orders.serviceType.${order.serviceType}`)}</dd>

      <dt className="font-semibold">{t("orders.columns.due")}</dt>
      <dd>{formatDay(order.dueDate, locale)}</dd>

      <dt className="font-semibold">{t("orders.detail.placedAt")}</dt>
      <dd>{formatDay(order.placedAt, locale)}</dd>
    </dl>
  );
}

function formatDay(value: string, locale: string): string {
  return new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(
    new Date(value.length === 10 ? `${value}T00:00:00` : value),
  );
}
