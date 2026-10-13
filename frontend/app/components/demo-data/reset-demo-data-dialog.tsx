import { useTranslation } from "react-i18next";
import { ConfirmDialog, ConfirmTone } from "~/components/confirm-dialog";

type ResetDemoDataDialogProps = {
  orderCount: number;
  onClose: () => void;
  onConfirm: () => void;
};

export function ResetDemoDataDialog({
  orderCount,
  onClose,
  onConfirm,
}: ResetDemoDataDialogProps) {
  const { t } = useTranslation();

  return (
    <ConfirmDialog
      title={t("demoData.title")}
      message={t("demoData.confirm", { orderCount })}
      confirmLabel={t("demoData.submit")}
      tone={ConfirmTone.Warning}
      onClose={onClose}
      onConfirm={onConfirm}
    />
  );
}
