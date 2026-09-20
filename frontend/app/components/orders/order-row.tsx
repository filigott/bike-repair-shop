import { useTranslation } from "react-i18next";
import { useLocale } from "~/hooks/use-locale";
import type { MaintenanceOrderSummary } from "~/services/endpoints/orders";

export function OrderRow({ order }: { order: MaintenanceOrderSummary }) {
  const { t } = useTranslation();
  const { locale } = useLocale();

  return (
    <tr>
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
        <span className="badge badge-ghost">
          {t(`orders.status.${order.status}`)}
        </span>
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
