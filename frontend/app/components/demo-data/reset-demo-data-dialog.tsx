import { useTranslation } from "react-i18next";

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
    <dialog
      ref={(node) => {
        node?.showModal();
      }}
      onClose={onClose}
      className="modal"
    >
      <div className="modal-box">
        <h2 className="text-lg font-bold">{t("demoData.title")}</h2>
        <p className="py-4">{t("demoData.confirm", { orderCount })}</p>
        <div className="modal-action">
          <form method="dialog">
            <button type="submit" className="btn btn-ghost">
              {t("common.cancel")}
            </button>
          </form>
          <button type="button" className="btn btn-warning" onClick={onConfirm}>
            {t("demoData.submit")}
          </button>
        </div>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button type="submit">{t("common.cancel")}</button>
      </form>
    </dialog>
  );
}
