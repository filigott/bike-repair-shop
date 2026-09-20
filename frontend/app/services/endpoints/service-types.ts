import { apiClient, unwrap } from "../http";
import type { components } from "../openapi.generated";

export type ServiceTypeItem = components["schemas"]["ServiceTypeItem"];

export function listServiceTypes(): Promise<ServiceTypeItem[]> {
  return unwrap(apiClient.GET("/api/service-types"));
}
