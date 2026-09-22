import { useTranslation } from "react-i18next";
import type { OrderAction } from "~/services/endpoints/orders";
import { type EditCommand, OrderIntent } from "~/types/intents/orders";

export type ActionTone = "primary" | "error";

// Class names are spelled out because Tailwind only emits what it finds literally in the source.
const styles: Record<OrderAction, { tone: ActionTone; button: string }> = {
  Start: { tone: "primary", button: "btn btn-soft btn-sm btn-primary" },
  Complete: { tone: "primary", button: "btn btn-soft btn-sm btn-primary" },
  Reopen: { tone: "primary", button: "btn btn-soft btn-sm btn-primary" },
  Cancel: { tone: "error", button: "btn btn-soft btn-sm btn-error" },
};

export function toneOf(action: OrderAction): ActionTone {
  return styles[action].tone;
}

export function OrderActionBar({
  actions,
  edits,
  onAction,
  onEdit,
}: {
  actions: OrderAction[];
  edits: readonly EditCommand[];
  onAction: (action: OrderAction) => void;
  onEdit: (command: EditCommand) => void;
}) {
  const { t } = useTranslation();
  const editLabels: Record<EditCommand, string> = {
    [OrderIntent.CorrectDetails]: t("orders.correctDetails.action"),
    [OrderIntent.ChangeService]: t("orders.changeService.action"),
    [OrderIntent.Reschedule]: t("orders.reschedule.action"),
  };

  if (actions.length === 0 && edits.length === 0) {
    return null;
  }

  return (
    <div className="flex flex-wrap gap-2">
      {actions.map((action) => (
        <button
          key={action}
          type="button"
          className={styles[action].button}
          onClick={() => onAction(action)}
        >
          {t(`orders.actions.${action}`)}
        </button>
      ))}
      {edits.map((command) => (
        <button
          key={command}
          type="button"
          className="btn btn-soft btn-sm btn-secondary"
          onClick={() => onEdit(command)}
        >
          {editLabels[command]}
        </button>
      ))}
    </div>
  );
}
