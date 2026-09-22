import { useTranslation } from "react-i18next";
import type { MaintenanceOrderSummary } from "~/services/endpoints/orders";

type DeleteOrderDialogProps = {
  order: MaintenanceOrderSummary;
  onClose: () => void;
  onConfirm: () => void;
};

export function DeleteOrderDialog({
  order,
  onClose,
  onConfirm,
}: DeleteOrderDialogProps) {
  const { t } = useTranslation();

  return (
    <dialog
      ref={(node) => {
        node?.showModal();
      }}
      onClose={onClose}
      className="modal"
    >
      <div className="modal-box">
        <h2 className="text-lg font-bold">{t("orders.delete.title")}</h2>
        <p className="py-4">
          {t("orders.delete.confirm", {
            reference: t("orders.reference", { number: order.orderNumber }),
            customer: order.customerName,
          })}
        </p>
        <div className="modal-action">
          <form method="dialog">
            <button type="submit" className="btn btn-ghost">
              {t("common.cancel")}
            </button>
          </form>
          <button type="button" className="btn btn-error" onClick={onConfirm}>
            {t("orders.delete.submit")}
          </button>
        </div>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button type="submit">{t("common.cancel")}</button>
      </form>
    </dialog>
  );
}
