import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { ordersHref } from "~/types/params/orders";

type OrdersPagerProps = {
  search: string;
  page: number;
  pageSize: number;
  totalCount: number;
};

export function OrdersPager({
  search,
  page,
  pageSize,
  totalCount,
}: OrdersPagerProps) {
  const { t } = useTranslation();
  const from = (page - 1) * pageSize + 1;

  return (
    <nav className="flex items-center gap-4">
      <span className="text-sm">
        {t("orders.paging.showing", {
          from,
          to: Math.min(page * pageSize, totalCount),
          total: totalCount,
        })}
      </span>
      {page > 1 && (
        <Link
          to={ordersHref({ search, page: page - 1 })}
          className="btn btn-sm ml-auto"
        >
          {t("orders.paging.previous")}
        </Link>
      )}
      {page * pageSize < totalCount && (
        <Link
          to={ordersHref({ search, page: page + 1 })}
          className="btn btn-sm last:ml-auto"
        >
          {t("orders.paging.next")}
        </Link>
      )}
    </nav>
  );
}
