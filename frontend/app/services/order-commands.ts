import type {
  MaintenanceOrderResponse,
  OrderAction,
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
import {
  optionalIntegerOf,
  optionalTextOf,
  serviceTypeOf,
  textOf,
} from "~/services/form-data";
import { orderDetailsFrom } from "~/services/order-form-data";
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
        ...orderDetailsFrom(form),
        version: optionalIntegerOf(form, "version"),
      });
    case OrderIntent.ChangeService:
      return changeOrderService(id, {
        serviceType: serviceTypeOf(form, "serviceType"),
        version: optionalIntegerOf(form, "version"),
      });
    case OrderIntent.Reschedule:
      return rescheduleOrder(id, {
        dueDate: optionalTextOf(form, "dueDate"),
        version: optionalIntegerOf(form, "version"),
      });
    case OrderIntent.Delete:
      return deleteOrder(id);
  }
}
