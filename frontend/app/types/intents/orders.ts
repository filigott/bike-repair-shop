export enum OrderIntent {
  CorrectDetails = "orders.correctDetails",
  ChangeService = "orders.changeService",
  Reschedule = "orders.reschedule",
  Transition = "orders.transition",
  Delete = "orders.delete",
}

export const editCommands = [
  OrderIntent.CorrectDetails,
  OrderIntent.ChangeService,
  OrderIntent.Reschedule,
] as const;

export type EditCommand = (typeof editCommands)[number];
