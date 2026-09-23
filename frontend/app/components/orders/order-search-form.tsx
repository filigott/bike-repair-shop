import { useTranslation } from "react-i18next";
import { Form, Link } from "react-router";

export function OrderSearchForm({ search }: { search: string }) {
  const { t } = useTranslation();

  return (
    <Form method="get" role="search" className="join">
      <input
        key={search}
        type="search"
        name="search"
        defaultValue={search}
        aria-label={t("orders.search.label")}
        placeholder={t("orders.search.placeholder")}
        className="input join-item"
      />
      <button type="submit" className="btn btn-primary join-item">
        {t("orders.search.submit")}
      </button>
      {search && (
        <Link to="/" className="btn btn-ghost join-item">
          {t("orders.search.clear")}
        </Link>
      )}
    </Form>
  );
}
