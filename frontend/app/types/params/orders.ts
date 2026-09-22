import {
  type EditCommand,
  editCommands,
  OrderIntent,
} from "~/types/intents/orders";

export enum OrderEditParam {
  Details = "details",
  Service = "service",
  DueDate = "due-date",
}

export const EDIT_PARAM = "edit";

const commands: Record<OrderEditParam, EditCommand> = {
  [OrderEditParam.Details]: OrderIntent.CorrectDetails,
  [OrderEditParam.Service]: OrderIntent.ChangeService,
  [OrderEditParam.DueDate]: OrderIntent.Reschedule,
};

export const editParams: Record<EditCommand, OrderEditParam> = {
  [OrderIntent.CorrectDetails]: OrderEditParam.Details,
  [OrderIntent.ChangeService]: OrderEditParam.Service,
  [OrderIntent.Reschedule]: OrderEditParam.DueDate,
};

export function editCommandOf(value: string | null): EditCommand | null {
  return commands[value as OrderEditParam] ?? null;
}

export function isEditCommand(intent: OrderIntent): intent is EditCommand {
  return editCommands.includes(intent as EditCommand);
}
