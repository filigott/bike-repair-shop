import { Fragment } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import type { OrderAction } from "~/services/endpoints/orders";
import { type EditCommand, OrderIntent } from "~/types/intents/orders";
import { EDIT_PARAM, editParams } from "~/types/params/orders";

export type ActionTone = "primary" | "error";

// Class names are spelled out because Tailwind only emits what it finds literally in the source.
const styles: Record<OrderAction, { tone: ActionTone; button: string }> = {
  Start: { tone: "primary", button: "btn btn-soft btn-primary w-full" },
  Complete: { tone: "primary", button: "btn btn-soft btn-primary w-full" },
  Reopen: { tone: "primary", button: "btn btn-soft btn-primary w-full" },
  Cancel: { tone: "error", button: "btn btn-soft btn-error w-full" },
};

export function toneOf(action: OrderAction): ActionTone {
  return styles[action].tone;
}

export function OrderActions({
  actions,
  edits,
  onAction,
  onDelete,
}: {
  actions: OrderAction[];
  edits: readonly EditCommand[];
  onAction: (action: OrderAction) => void;
  onDelete: () => void;
}) {
  const { t } = useTranslation();
  const editLabels: Record<EditCommand, string> = {
    [OrderIntent.CorrectDetails]: t("orders.correctDetails.action"),
    [OrderIntent.ChangeService]: t("orders.changeService.action"),
    [OrderIntent.Reschedule]: t("orders.reschedule.action"),
  };

  const groups = [
    {
      key: "lifecycle",
      buttons: actions.map((action) => (
        <button
          key={action}
          type="button"
          className={styles[action].button}
          onClick={() => onAction(action)}
        >
          {t(`orders.actions.${action}`)}
        </button>
      )),
    },
    {
      key: "edit",
      buttons: edits.map((command) => (
        <Link
          key={command}
          to={`?${EDIT_PARAM}=${editParams[command]}`}
          className="btn btn-soft btn-secondary w-full"
        >
          {editLabels[command]}
        </Link>
      )),
    },
    {
      key: "remove",
      buttons: [
        <button
          key="delete"
          type="button"
          className="btn btn-outline btn-error w-full"
          onClick={onDelete}
        >
          {t("orders.delete.action")}
        </button>,
      ],
    },
  ].filter((group) => group.buttons.length > 0);

  return (
    <div className="flex flex-col gap-3">
      {groups.map((group, index) => (
        <Fragment key={group.key}>
          {index > 0 && <div className="border-base-300 border-t" />}
          <div className="flex flex-col gap-2">{group.buttons}</div>
        </Fragment>
      ))}
    </div>
  );
}
