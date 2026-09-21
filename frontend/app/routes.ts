import { index, type RouteConfig, route } from "@react-router/dev/routes";

export default [
  index("routes/orders.tsx"),
  route("orders/place", "routes/place-order.tsx"),
  route("orders/:id", "routes/order-detail.tsx"),
] satisfies RouteConfig;
