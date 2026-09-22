import { useTranslation } from "react-i18next";

type ConfirmDialogProps = {
  title: string;
  message: string;
  confirmLabel: string;
  tone: "primary" | "error";
  onClose: () => void;
  onConfirm: () => void;
};

export function ConfirmDialog({
  title,
  message,
  confirmLabel,
  tone,
  onClose,
  onConfirm,
}: ConfirmDialogProps) {
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
        <h2 className="text-lg font-bold">{title}</h2>
        <p className="py-4">{message}</p>
        <div className="modal-action">
          <form method="dialog">
            <button type="submit" className="btn btn-ghost">
              {t("common.cancel")}
            </button>
          </form>
          <button
            type="button"
            className={tone === "error" ? "btn btn-error" : "btn btn-primary"}
            onClick={onConfirm}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button type="submit">{t("common.cancel")}</button>
      </form>
    </dialog>
  );
}
