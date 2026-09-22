import { useTranslation } from "react-i18next";
import type { MaintenanceOrderResponse } from "~/services/endpoints/orders";

export function OrderContactCard({
  order,
}: {
  order: MaintenanceOrderResponse;
}) {
  const { t } = useTranslation();

  return (
    <dl className="grid flex-1 grid-cols-[auto_1fr] content-between gap-x-6 gap-y-4 rounded-box bg-base-200 p-6 text-base">
      <dt className="text-base-content/60">{t("orders.fields.phoneNumber")}</dt>
      <dd className="font-medium">{order.phoneNumber}</dd>

      <dt className="text-base-content/60">{t("orders.fields.email")}</dt>
      <dd className="font-medium">{order.email}</dd>

      <dt className="text-base-content/60">{t("orders.fields.bikeBrand")}</dt>
      <dd className="font-medium">{order.bikeBrand}</dd>

      <dt className="text-base-content/60">{t("orders.fields.notes")}</dt>
      <dd className="font-medium">{order.notes}</dd>
    </dl>
  );
}
