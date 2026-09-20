import { apiClient, unwrap } from "../http";
import type { components, paths } from "../openapi.generated";

export type MaintenanceOrderSummary =
  components["schemas"]["MaintenanceOrderSummary"];
export type MaintenanceOrderResponse =
  components["schemas"]["MaintenanceOrderResponse"];
export type PlaceOrderRequest = components["schemas"]["PlaceOrderRequest"];
export type PagedOrders =
  components["schemas"]["PagedResponseOfMaintenanceOrderSummary"];
export type ListOrdersParams = NonNullable<
  paths["/api/orders"]["get"]["parameters"]["query"]
>;
export type OrderStatus = components["schemas"]["OrderStatus"];
export type ServiceType = components["schemas"]["ServiceType"];

export function listOrders(
  params: ListOrdersParams = {},
): Promise<PagedOrders> {
  return unwrap(apiClient.GET("/api/orders", { params: { query: params } }));
}

export function placeOrder(
  order: PlaceOrderRequest,
): Promise<MaintenanceOrderResponse> {
  return unwrap(apiClient.POST("/api/orders", { body: order }));
}
