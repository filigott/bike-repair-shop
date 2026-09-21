import { useTranslation } from "react-i18next";
import type { MaintenanceOrderResponse } from "~/services/endpoints/orders";

export function OrderContactCard({
  order,
}: {
  order: MaintenanceOrderResponse;
}) {
  const { t } = useTranslation();

  return (
    <dl className="grid grid-cols-2 gap-2 rounded-box bg-base-200 p-4">
      <dt className="font-semibold">{t("orders.fields.customerName")}</dt>
      <dd>{order.customerName}</dd>

      <dt className="font-semibold">{t("orders.fields.phoneNumber")}</dt>
      <dd>{order.phoneNumber}</dd>

      <dt className="font-semibold">{t("orders.fields.email")}</dt>
      <dd>{order.email}</dd>

      <dt className="font-semibold">{t("orders.fields.bikeBrand")}</dt>
      <dd>{order.bikeBrand}</dd>

      {order.notes && (
        <>
          <dt className="font-semibold">{t("orders.fields.notes")}</dt>
          <dd>{order.notes}</dd>
        </>
      )}
    </dl>
  );
}
