import { useTranslation } from "react-i18next";
import type { OrderAction } from "~/services/endpoints/orders";

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
  onSelect,
}: {
  actions: OrderAction[];
  onSelect: (action: OrderAction) => void;
}) {
  const { t } = useTranslation();

  if (actions.length === 0) {
    return null;
  }

  return (
    <div className="flex flex-wrap gap-2">
      {actions.map((action) => (
        <button
          key={action}
          type="button"
          className={styles[action].button}
          onClick={() => onSelect(action)}
        >
          {t(`orders.actions.${action}`)}
        </button>
      ))}
    </div>
  );
}
