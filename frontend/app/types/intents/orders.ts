export enum OrderIntent {
  CorrectDetails = "orders.correctDetails",
  ChangeService = "orders.changeService",
  Reschedule = "orders.reschedule",
  Transition = "orders.transition",
  Delete = "orders.delete",
}

const intents = Object.values(OrderIntent);

export function orderIntentOf(value: string): OrderIntent {
  const intent = intents.find((candidate) => candidate === value);

  if (!intent) {
    throw new Error(`Unknown order intent: ${value}`);
  }

  return intent;
}

export const editCommands = [
  OrderIntent.CorrectDetails,
  OrderIntent.ChangeService,
  OrderIntent.Reschedule,
] as const;

export type EditCommand = (typeof editCommands)[number];

export const editCommandLabels = {
  [OrderIntent.CorrectDetails]: "orders.correctDetails.action",
  [OrderIntent.ChangeService]: "orders.changeService.action",
  [OrderIntent.Reschedule]: "orders.reschedule.action",
} as const satisfies Record<EditCommand, string>;
