import { Fragment } from "react";
import { useTranslation } from "react-i18next";
import { OrderStatusBadge } from "~/components/orders/order-status-badge";
import type { OrderStatusItem } from "~/services/endpoints/order-statuses";

export function OrderStatusLegend({
  statuses,
}: {
  statuses: OrderStatusItem[];
}) {
  const { t } = useTranslation();

  return (
    <details className="rounded-box bg-base-200 px-4 py-3">
      <summary className="flex cursor-pointer flex-wrap items-center gap-2">
        <span className="font-semibold text-base-content/60 text-sm uppercase tracking-wide">
          {t("orders.legend.title")}
        </span>
        {statuses.map((item) => (
          <OrderStatusBadge key={item.status} status={item.status} />
        ))}
      </summary>

      <dl className="mt-3 grid grid-cols-[auto_1fr] items-center gap-x-4 gap-y-2 border-base-300 border-t pt-3">
        {statuses.map((item) => (
          <Fragment key={item.status}>
            <dt>
              <OrderStatusBadge status={item.status} />
            </dt>
            <dd className="flex flex-wrap items-center gap-2 text-base-content/70 text-sm">
              {item.allowedTransitions.length === 0 ? (
                t("orders.legend.final")
              ) : (
                <>
                  {t("orders.legend.becomes")}
                  {item.allowedTransitions.map((target) => (
                    <OrderStatusBadge key={target} status={target} />
                  ))}
                </>
              )}
            </dd>
          </Fragment>
        ))}
      </dl>
    </details>
  );
}
