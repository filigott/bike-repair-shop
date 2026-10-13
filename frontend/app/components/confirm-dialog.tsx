import { useTranslation } from "react-i18next";

export enum ConfirmTone {
  Primary = "primary",
  Warning = "warning",
  Error = "error",
}

// Class names are spelled out because Tailwind only emits what it finds literally in the source.
const confirmButtons: Record<ConfirmTone, string> = {
  [ConfirmTone.Primary]: "btn btn-primary",
  [ConfirmTone.Warning]: "btn btn-warning",
  [ConfirmTone.Error]: "btn btn-error",
};

type ConfirmDialogProps = {
  title: string;
  message: string;
  confirmLabel: string;
  tone: ConfirmTone;
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
            className={confirmButtons[tone]}
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
