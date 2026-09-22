import { useTranslation } from "react-i18next";
import { Form, useActionData, useNavigation } from "react-router";
import type { ActionResult } from "~/types/action-result";
import { isActionFailure } from "~/types/action-result";
import type { OrderIntent } from "~/types/intents/orders";
import type { OrderFieldErrors, OrderFormField } from "./order-fields";

type CommandDialogProps = {
  intent: OrderIntent;
  title: string;
  submitLabel: string;
  onClose: () => void;
  children: (errors: OrderFieldErrors) => React.ReactNode;
};

export function CommandDialog({
  intent,
  title,
  submitLabel,
  onClose,
  children,
}: CommandDialogProps) {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const result = useActionData<ActionResult<null, OrderFormField>>();
  const failure = result && isActionFailure(result) ? result : undefined;
  const saving = navigation.state !== "idle";

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
        <Form method="post" className="flex flex-col gap-3 pt-4">
          <input type="hidden" name="intent" value={intent} />

          {failure && (
            <p role="alert" className="alert alert-error">
              {failure.detail ?? failure.title}
            </p>
          )}

          {children(failure?.fieldErrors ?? {})}

          <div className="modal-action">
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              {t("common.cancel")}
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? t("common.saving") : submitLabel}
            </button>
          </div>
        </Form>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button type="submit">{t("common.cancel")}</button>
      </form>
    </dialog>
  );
}
