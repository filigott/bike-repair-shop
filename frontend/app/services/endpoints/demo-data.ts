import { apiClient, unwrap } from "../http";
import type { components } from "../openapi.generated";

export type DemoDataStatus = components["schemas"]["DemoDataStatus"];

export function getDemoDataStatus(): Promise<DemoDataStatus> {
  return unwrap(apiClient.GET("/api/demo-data"));
}

export function resetDemoData(): Promise<DemoDataStatus> {
  return unwrap(apiClient.POST("/api/demo-data/reset"));
}
