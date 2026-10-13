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

const editParamValues = Object.values(OrderEditParam);

export function editCommandOf(value: string | null): EditCommand | null {
  const param = editParamValues.find((candidate) => candidate === value);

  return param ? commands[param] : null;
}

export function isEditCommand(intent: OrderIntent): intent is EditCommand {
  return editCommands.includes(intent as EditCommand);
}

export enum OrdersParam {
  Search = "search",
  Page = "page",
}

export type OrdersQuery = { search: string; page: number };

export function ordersQueryOf(params: URLSearchParams): OrdersQuery {
  const page = Number(params.get(OrdersParam.Page));

  return {
    search: params.get(OrdersParam.Search)?.trim() ?? "",
    page: Number.isSafeInteger(page) && page > 0 ? page : 1,
  };
}

export function ordersHref({ search, page }: OrdersQuery): string {
  const params = new URLSearchParams();
  if (search) {
    params.set(OrdersParam.Search, search);
  }
  params.set(OrdersParam.Page, String(page));
  return `?${params}`;
}
