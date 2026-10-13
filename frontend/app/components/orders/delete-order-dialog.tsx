import { useTranslation } from "react-i18next";
import { ConfirmDialog, ConfirmTone } from "~/components/confirm-dialog";
import type { MaintenanceOrderSummary } from "~/services/endpoints/orders";

type DeletableOrder = Pick<
  MaintenanceOrderSummary,
  "orderNumber" | "customerName"
>;

type DeleteOrderDialogProps = {
  order: DeletableOrder;
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
    <ConfirmDialog
      title={t("orders.delete.title")}
      message={t("orders.delete.confirm", {
        reference: t("orders.reference", { number: order.orderNumber }),
        customer: order.customerName,
      })}
      confirmLabel={t("orders.delete.submit")}
      tone={ConfirmTone.Error}
      onClose={onClose}
      onConfirm={onConfirm}
    />
  );
}
