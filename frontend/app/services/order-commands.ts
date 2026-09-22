import type {
  MaintenanceOrderResponse,
  OrderAction,
  ServiceType,
} from "~/services/endpoints/orders";
import {
  cancelOrder,
  changeOrderService,
  completeOrder,
  correctOrderDetails,
  deleteOrder,
  reopenOrder,
  rescheduleOrder,
  startOrder,
} from "~/services/endpoints/orders";
import { optionalTextOf, textOf } from "~/services/form-data";
import { OrderIntent } from "~/types/intents/orders";

const transitions: Record<
  OrderAction,
  (id: string) => Promise<MaintenanceOrderResponse>
> = {
  Start: startOrder,
  Complete: completeOrder,
  Cancel: cancelOrder,
  Reopen: reopenOrder,
};

function isOrderAction(value: string): value is OrderAction {
  return Object.hasOwn(transitions, value);
}

export function runOrderCommand(
  id: string,
  intent: OrderIntent,
  form: FormData,
): Promise<unknown> {
  switch (intent) {
    case OrderIntent.Transition: {
      const action = textOf(form, "action");

      if (!isOrderAction(action)) {
        throw new Error(`Unknown order action: ${action}`);
      }

      return transitions[action](id);
    }
    case OrderIntent.CorrectDetails:
      return correctOrderDetails(id, {
        customerName: textOf(form, "customerName"),
        phoneNumber: textOf(form, "phoneNumber"),
        email: textOf(form, "email"),
        bikeBrand: textOf(form, "bikeBrand"),
        notes: optionalTextOf(form, "notes"),
      });
    case OrderIntent.ChangeService:
      return changeOrderService(id, {
        serviceType: textOf(form, "serviceType") as ServiceType,
      });
    case OrderIntent.Reschedule:
      return rescheduleOrder(id, { dueDate: optionalTextOf(form, "dueDate") });
    case OrderIntent.Delete:
      return deleteOrder(id);
  }
}
