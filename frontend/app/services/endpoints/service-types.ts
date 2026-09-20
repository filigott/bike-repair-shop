import { request } from "../http";
import type { components } from "../openapi.generated";

export type ServiceTypeItem = components["schemas"]["ServiceTypeItem"];

export function listServiceTypes(): Promise<ServiceTypeItem[]> {
  return request<ServiceTypeItem[]>("/api/service-types");
}
