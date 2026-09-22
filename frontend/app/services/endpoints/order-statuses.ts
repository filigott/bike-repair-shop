import { apiClient, unwrap } from "../http";
import type { components } from "../openapi.generated";

export type OrderStatusItem = components["schemas"]["OrderStatusItem"];

export function listOrderStatuses(): Promise<OrderStatusItem[]> {
  return unwrap(apiClient.GET("/api/order-statuses"));
}
