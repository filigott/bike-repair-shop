import { apiClient, unwrap } from "../http";
import type { components, paths } from "../openapi.generated";

export type MaintenanceOrderSummary =
  components["schemas"]["MaintenanceOrderSummary"];
export type MaintenanceOrderResponse =
  components["schemas"]["MaintenanceOrderResponse"];
export type PlaceOrderRequest = components["schemas"]["PlaceOrderRequest"];
export type CorrectOrderDetailsRequest =
  components["schemas"]["CorrectOrderDetailsRequest"];
export type ChangeOrderServiceRequest =
  components["schemas"]["ChangeOrderServiceRequest"];
export type RescheduleOrderRequest =
  components["schemas"]["RescheduleOrderRequest"];
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

export function getOrder(id: string): Promise<MaintenanceOrderResponse> {
  return unwrap(
    apiClient.GET("/api/orders/{id}", { params: { path: { id } } }),
  );
}

export function correctOrderDetails(
  id: string,
  details: CorrectOrderDetailsRequest,
): Promise<MaintenanceOrderResponse> {
  return unwrap(
    apiClient.PUT("/api/orders/{id}/details", {
      params: { path: { id } },
      body: details,
    }),
  );
}

export function changeOrderService(
  id: string,
  service: ChangeOrderServiceRequest,
): Promise<MaintenanceOrderResponse> {
  return unwrap(
    apiClient.PUT("/api/orders/{id}/service", {
      params: { path: { id } },
      body: service,
    }),
  );
}

export function rescheduleOrder(
  id: string,
  schedule: RescheduleOrderRequest,
): Promise<MaintenanceOrderResponse> {
  return unwrap(
    apiClient.PUT("/api/orders/{id}/due-date", {
      params: { path: { id } },
      body: schedule,
    }),
  );
}
